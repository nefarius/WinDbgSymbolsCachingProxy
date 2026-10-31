using System.Diagnostics;
using System.Net;

using FastEndpoints;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using MongoDB.Driver;
using MongoDB.Entities;
using Polly.CircuitBreaker;
using Polly.Timeout;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Mappers;
using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Serves symbol downloads, mirroring the behavior of the Microsoft Symbol Server.
/// </summary>
public sealed class SymbolsDownloadEndpoint(
    DB db,
    ILogger<SymbolsDownloadEndpoint> logger,
    IHttpClientFactory clientFactory,
    IOptions<ServiceConfig> options,
    IMemoryCache mc,
    SymbolAliasLookupService aliasLookup)
    : Endpoint<SymbolsRequest>
{
    private static readonly ActivitySource ActivitySource = new(TracingSources.AppActivitySourceName);

    private static readonly SymbolsEntityMapper Mapper = new();

    /// <summary>
    ///     Blobs larger than this are not kept in the in-memory cache.
    /// </summary>
    private const int MaxMemoryCachedBlobBytes = 32 * 1024 * 1024;

    private static readonly TimeSpan UpstreamBodyTimeout = TimeSpan.FromMinutes(10);

    private static readonly TimeSpan PositiveCacheLifetime = TimeSpan.FromHours(12);

    /// <summary>
    ///     Atomically stamps access metadata without rewriting (and potentially reverting) the whole document.
    /// </summary>
    private async Task StampAccessAsync(string indexPrefix, string fileName)
    {
        try
        {
            string prefix = indexPrefix.ToLowerInvariant();
            string name = fileName.ToLowerInvariant();

            await db.Update<SymbolsEntity>()
                .Match(e => e.IndexPrefix == prefix && e.FileName == name && e.AccessedCount == null)
                .Modify(b => b.Set(e => e.AccessedCount, 0UL))
                .ExecuteAsync(CancellationToken.None);

            await db.Update<SymbolsEntity>()
                .Match(e => e.IndexPrefix == prefix && e.FileName == name)
                .Modify(b => b.Set(e => e.LastAccessedAt, DateTime.UtcNow))
                .Modify(b => b.Inc(e => e.AccessedCount, 1UL))
                .ExecuteAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to persist access metadata for {IndexPrefix}", indexPrefix);
        }
    }

    private static bool IsDuplicateKey(MongoWriteException ex)
    {
        return ex.WriteError?.Category == ServerErrorCategory.DuplicateKey;
    }

    /// <summary>
    ///     Configures the GET route for symbol download at "/download/symbols/{Symbol}/{SymbolKey}/{FileName}" with anonymous access and the "Symbols" tag.
    /// </summary>
    public override void Configure()
    {
        Get("/download/symbols/{Symbol}/{SymbolKey}/{FileName}");
        // Anonymous in Basic-auth mode (AllowAnonymous is applied in Program.cs so FastEndpoints
        // does not also emit RequireAuthenticatedUser). Requires symbols.download in OIDC mode.
        // Non-interactive clients (WinDbg, CI) must supply an API key via X-Api-Key header or ?apiKey= query param.
        Policies(Core.Permissions.SymbolsDownload);
        Options(x => x.WithTags("Symbols"));
    }

    /// <summary>
    ///     Processes the request for downloading and caching a symbol file. The method attempts to retrieve the symbol from
    ///     memory cache, database, or an upstream symbol server.
    ///     It ensures the symbol is properly cached for future requests and streams the file to the requester.
    /// </summary>
    /// <param name="req">The symbol request containing details such as IndexPrefix, Symbol, SymbolKey, and FileName.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation of handling the symbol request.</returns>
    public override async Task HandleAsync(SymbolsRequest req, CancellationToken ct)
    {
        Stopwatch sw = Stopwatch.StartNew();
        string requestPath = HttpContext.Request.Path;
        string? remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        string? userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        bool hadRange = HttpContext.Request.Headers.ContainsKey("Range");

        await using CancellationTokenRegistration abortRegistration = HttpContext.RequestAborted.Register(() =>
        {
            logger.LogDebug(
                "Client disconnected for {RequestPath} (remote={RemoteIp}, ua={UserAgent}, range={HadRange}, elapsedMs={Elapsed})",
                requestPath, remoteIp, userAgent, hadRange, sw.ElapsedMilliseconds);
        });

        Activity? parentActivity = Activity.Current;

        parentActivity?.AddTag("request.IndexPrefix", req.IndexPrefix);
        parentActivity?.AddTag("request.FileName", req.FileName);

        // try to probe memory cache before querying DB
        if (mc.TryGetValue(req.ToString(), out SymbolsEntityDto? cachedEntity) && cachedEntity is not null)
        {
            logger.LogDebug("Found cached copy in memory for {Request}", req.ToString());

            if (cachedEntity.NotFoundAt.HasValue || cachedEntity.Blob is null)
            {
                if (await TryServeFromCustomAliasAsync(req, ct))
                {
                    return;
                }

                await Send.NotFoundAsync(ct);
                return;
            }

            logger.LogDebug("Returning memory-cached copy for {Entity}", cachedEntity.ToString());

            string memoryStreamName = ResolveStreamFileNameFromDto(cachedEntity, req);
            try
            {
                await Send.BytesAsync(cachedEntity.Blob, memoryStreamName, enableRangeProcessing: true, cancellation: ct);
            }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
            {
                return;
            }

            await StampAccessAsync(cachedEntity.IndexPrefix, cachedEntity.FileName);

            return;
        }

        SymbolsEntity? existingSymbol;
        using (Activity? querySymbolInDbActivity = ActivitySource.StartActivity("Find"))
        {
            existingSymbol = (await db.Find<SymbolsEntity>()
                    .ManyAsync(lr =>
                            lr.Eq(r => r.IndexPrefix, req.IndexPrefix.ToLowerInvariant()) &
                            lr.Eq(r => r.FileName, req.FileName.ToLowerInvariant())
                        , CancellationToken.None)
                ).FirstOrDefault();
        }

        // cached entry found
        if (existingSymbol is not null)
        {
            logger.LogDebug("Found cached symbol {Symbol}", existingSymbol.ToString());

            if (existingSymbol.NotFoundAt.HasValue)
            {
                if (existingSymbol.NotFoundAt.Value.Add(options.Value.UpstreamRecheckPeriod) > DateTime.UtcNow)
                {
                    if (await TryServeFromCustomAliasAsync(req, ct))
                    {
                        return;
                    }

                    logger.LogInformation("Cached symbol marked as not found");

                    CacheSymbolInMemory(req, existingSymbol);

                    await Send.NotFoundAsync(ct);
                    return;
                }

                // Expired not-found: skip blob download, fall through to upstream recheck
                logger.LogInformation("Cached not-found entry expired, rechecking upstream");
            }
            else
            {
                // deliver cached copy of symbol blob
                using MemoryStream ms = new();

                try
                {
                    await existingSymbol.Data(db).DownloadAsync(ms, cancellation: CancellationToken.None);
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogWarning(ex,
                        "Cached metadata found but blob missing or unreadable, re-downloading from upstream");
                    if (existingSymbol.IsCustom)
                    {
                        // Never discard metadata (aliases, upload info) of manually uploaded symbols.
                        logger.LogError(ex, "Blob of custom symbol {Symbol} is missing", existingSymbol.ToString());
                        await Send.NotFoundAsync(ct);
                        return;
                    }

                    await db.DeleteAsync<SymbolsEntity>(existingSymbol.ID, CancellationToken.None);
                    existingSymbol = null;
                }
                catch (MongoException ex)
                {
                    logger.LogWarning(ex,
                        "MongoDB error while reading cached blob, not evicting metadata; caller may retry");
                    return;
                }

                if (existingSymbol is not null)
                {
                    byte[] blob = ms.ToArray();
                    CacheSymbolInMemory(req, existingSymbol, blob);

                    SymbolsEntity stampTarget = existingSymbol;
                    _ = StampAccessAsync(stampTarget.IndexPrefix, stampTarget.FileName);

                    try
                    {
                        logger.LogDebug("Returning cached copy");
                        string streamName = ResolveStreamFileName(existingSymbol, req);
                        await Send.StreamAsync(new MemoryStream(blob), streamName, enableRangeProcessing: true, cancellation: ct);
                    }
                    catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
                    {
                        // benign client disconnect; already logged by the RequestAborted callback
                    }
                    catch (Exception ex)
                    {
                        if (!HttpContext.Response.HasStarted)
                        {
                            logger.LogWarning(ex, "Error while streaming cached symbol to client");
                            HttpContext.Response.StatusCode = 500;
                            await HttpContext.Response.WriteAsync("Error while streaming cached symbol to client.", ct);
                            return;
                        }

                        logger.LogWarning(ex, "Error after response started while streaming {RequestPath}", requestPath);
                    }

                    return;
                }
            }
        }

        if (await TryServeFromCustomAliasAsync(req, ct))
        {
            return;
        }

        HttpClient client = clientFactory.CreateClient("MicrosoftSymbolServer");

        HttpResponseMessage? response = null;

        try
        {
            response = await client.GetAsync(
                $"download/symbols/{Uri.EscapeDataString(req.Symbol)}/{Uri.EscapeDataString(req.SymbolKey)}/{Uri.EscapeDataString(req.FileName)}",
                HttpCompletionOption.ResponseHeadersRead,
                ct);
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Client disconnected during upstream fetch for {Request}", req.ToString());
            return;
        }
        catch (Exception ex) when (ex is BrokenCircuitException or TimeoutRejectedException or HttpRequestException)
        {
            // Resilience pipeline gave up (open circuit, timeout) or transport failed: report an upstream failure, not a 500.
            logger.LogWarning(ex, "Upstream symbol fetch failed for {Request}", req.ToString());
            if (!HttpContext.Response.HasStarted)
            {
                HttpContext.Response.StatusCode = 502;
                await HttpContext.Response.WriteAsync("Upstream symbol server unavailable.", CancellationToken.None);
            }

            return;
        }

        if (response is null)
        {
            logger.LogError("Upstream symbol client returned no HTTP response for {Request}", req.ToString());
            if (!HttpContext.Response.HasStarted)
            {
                HttpContext.Response.StatusCode = 502;
                await HttpContext.Response.WriteAsync("Upstream symbol client returned no response.", ct);
            }

            return;
        }

        try
        {
            SymbolsEntity newSymbol = existingSymbol ?? new SymbolsEntity
            {
                CreatedAt = DateTime.UtcNow,
                SymbolKey = req.SymbolKey.ToLowerInvariant(),
                FileName = req.FileName.ToLowerInvariant(),
                IndexPrefix = req.IndexPrefix.ToLowerInvariant()
            };

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                {
                    logger.LogInformation("Requested symbol {Symbol} not found upstream", req.ToString());

                    if (await TryServeFromCustomAliasAsync(req, ct))
                    {
                        return;
                    }

                    if (existingSymbol is null)
                    {
                        newSymbol.NotFoundAt = DateTime.UtcNow;
                        try
                        {
                            await db.SaveAsync(newSymbol, CancellationToken.None);
                        }
                        catch (MongoWriteException ex) when (IsDuplicateKey(ex))
                        {
                            logger.LogDebug("Concurrent request already stored not-found marker for {Request}", req);
                        }

                        CacheSymbolInMemory(req, newSymbol);
                    }
                    else
                    {
                        existingSymbol.NotFoundAt = DateTime.UtcNow;
                        await db.SaveAsync(existingSymbol, CancellationToken.None);
                        CacheSymbolInMemory(req, existingSymbol);
                    }

                    if (!HttpContext.Response.HasStarted)
                    {
                        await Send.NotFoundAsync(ct);
                    }
                }
                else
                {
                    logger.LogWarning("Upstream returned {StatusCode} for {Symbol}; not caching",
                        response.StatusCode, req.ToString());

                    if (!HttpContext.Response.HasStarted)
                    {
                        HttpContext.Response.StatusCode = 502;
                        await HttpContext.Response.WriteAsync(
                            $"Upstream symbol server returned {(int)response.StatusCode} {response.ReasonPhrase}.", ct);
                    }
                }

                return;
            }

            if (response.RequestMessage?.RequestUri is null)
            {
                logger.LogError("Upstream response is missing request message or URI");
                if (!HttpContext.Response.HasStarted)
                {
                    HttpContext.Response.StatusCode = 502;
                    await HttpContext.Response.WriteAsync("Invalid upstream response.", ct);
                }

                return;
            }

            // grab the filename we got after 302 got resolved
            string upstreamFilename = Path.GetFileName(response.RequestMessage.RequestUri.AbsolutePath);

            if (string.IsNullOrEmpty(upstreamFilename))
            {
                logger.LogWarning("Failed to extract upstream filename");

                // fallback value
                upstreamFilename = req.FileName.ToLowerInvariant();
            }

            logger.LogInformation("Got requested symbol {Symbol} ({Filename}), caching", req.ToString(),
                upstreamFilename);

            // cache in memory because we need to return it to database AND requester
            using MemoryStream cache = new();
            try
            {
                using CancellationTokenSource bodyCts = new(UpstreamBodyTimeout);
                await using Stream upstreamContent = await response.Content.ReadAsStreamAsync(bodyCts.Token);
                await upstreamContent.CopyToAsync(cache, bodyCts.Token);
            }
            catch (Exception ex) when (ex is BrokenCircuitException or TimeoutRejectedException or HttpRequestException
                                           or IOException ||
                                       (ex is OperationCanceledException &&
                                        !HttpContext.RequestAborted.IsCancellationRequested))
            {
                logger.LogWarning(ex, "Reading upstream symbol body failed for {Request}", req.ToString());
                if (!HttpContext.Response.HasStarted)
                {
                    HttpContext.Response.StatusCode = 502;
                    await HttpContext.Response.WriteAsync("Upstream symbol server unavailable.", CancellationToken.None);
                }

                return;
            }

            cache.Position = 0;

            newSymbol.UpstreamFileName = upstreamFilename;
            newSymbol.NotFoundAt = DateTime.MinValue;

            bool persisted = true;
            try
            {
                await db.SaveAsync(newSymbol, CancellationToken.None);
            }
            catch (MongoWriteException ex) when (IsDuplicateKey(ex))
            {
                // A concurrent request for the same symbol won the race; just serve our copy.
                persisted = false;
                logger.LogDebug("Concurrent request already stored {Request}; serving without persisting", req);
            }

            if (persisted)
            {
                try
                {
                    await newSymbol.Data(db).UploadAsync(cache, cancellation: CancellationToken.None);
                }
                catch
                {
                    if (existingSymbol is null)
                    {
                        await db.DeleteAsync<SymbolsEntity>(newSymbol.ID, CancellationToken.None);
                    }

                    throw;
                }

                newSymbol.NotFoundAt = null;
                newSymbol.LastAccessedAt = DateTime.UtcNow;
                newSymbol.AccessedCount = 1;
                await db.SaveAsync(newSymbol, CancellationToken.None);

                // save in memory cache to take the load off of the DB
                if (cache.Length <= MaxMemoryCachedBlobBytes)
                {
                    CacheSymbolInMemory(req, newSymbol, cache.ToArray());
                }
            }

            cache.Position = 0;

            try
            {
                await Send.StreamAsync(cache, upstreamFilename, enableRangeProcessing: true, cancellation: ct);
            }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
            {
                // benign client disconnect; already logged by the RequestAborted callback
            }
            catch (Exception ex)
            {
                if (!HttpContext.Response.HasStarted)
                {
                    HttpContext.Response.StatusCode = 500;
                    await HttpContext.Response.WriteAsync("Error while sending symbol file.", ct);
                }

                logger.LogError(ex, "Failed to stream symbol file for {RequestPath}", requestPath);
            }
        }
        finally
        {
            response?.Dispose();
        }
    }

    /// <summary>
    ///     Streams a custom symbol whose canonical path differs from <paramref name="req" /> but lists the requested symbol
    ///     name in <see cref="SymbolsEntity.AlternateRequestSymbols" />.
    /// </summary>
    private async Task<bool> TryServeFromCustomAliasAsync(SymbolsRequest req, CancellationToken ct)
    {
        SymbolsEntity? canonical = await aliasLookup.FindCustomSymbolByRequestPathAsync(
            req.SymbolKey.ToLowerInvariant(), req.Symbol.ToLowerInvariant(), ct);
        if (canonical is null)
        {
            return false;
        }

        logger.LogInformation(
            "Serving custom symbol for {Request} via alternate request name (canonical {Canonical})",
            req, canonical);

        using MemoryStream ms = new();

        try
        {
            await canonical.Data(db).DownloadAsync(ms, cancellation: ct);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Custom symbol metadata found but blob missing for {Canonical}", canonical);
            return false;
        }
        catch (MongoException ex)
        {
            logger.LogWarning(ex, "MongoDB error while reading custom symbol blob for {Canonical}", canonical);
            return false;
        }

        byte[] blob = ms.ToArray();
        CacheSymbolInMemory(req, canonical, blob);

        _ = StampAccessAsync(canonical.IndexPrefix, canonical.FileName);

        string streamName = ResolveStreamFileName(canonical, req);

        try
        {
            await Send.StreamAsync(new MemoryStream(blob), streamName, enableRangeProcessing: true, cancellation: ct);
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            // benign client disconnect; already logged by the RequestAborted callback
            return true;
        }
        catch (Exception ex)
        {
            if (!HttpContext.Response.HasStarted)
            {
                logger.LogWarning(ex, "Error while streaming alias-resolved symbol to client");
                HttpContext.Response.StatusCode = 500;
                await HttpContext.Response.WriteAsync("Error while streaming cached symbol to client.", ct);
            }
            else
            {
                logger.LogWarning(ex, "Error after response started while streaming {RequestPath}", HttpContext.Request.Path);
            }

            return true;
        }

        await TryDeleteShadowedUpstreamPlaceholderAsync(req, ct);
        return true;
    }

    private async Task TryDeleteShadowedUpstreamPlaceholderAsync(SymbolsRequest req, CancellationToken ct)
    {
        SymbolsEntity? stale = (await db.Find<SymbolsEntity>()
                .ManyAsync(lr =>
                        lr.Eq(r => r.IndexPrefix, req.IndexPrefix.ToLowerInvariant()) &
                        lr.Eq(r => r.FileName, req.FileName.ToLowerInvariant())
                    , ct))
            .FirstOrDefault();

        if (stale is null || stale.IsCustom || !stale.NotFoundAt.HasValue)
        {
            return;
        }

        try
        {
            await db.DeleteAsync<SymbolsEntity>(stale.ID, CancellationToken.None);
            logger.LogInformation("Removed redundant upstream not-found row for {Request}", req);
            mc.Remove(req.ToString());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete redundant not-found row {Id}", stale.ID);
        }
    }

    private static string ResolveStreamFileName(SymbolsEntity canonical, SymbolsRequest req)
    {
        bool exactPath = canonical.IndexPrefix.Equals(req.IndexPrefix, StringComparison.OrdinalIgnoreCase) &&
                         canonical.FileName.Equals(req.FileName, StringComparison.OrdinalIgnoreCase);
        if (exactPath)
        {
            return canonical.UpstreamFileName ?? canonical.FileName;
        }

        return req.FileName;
    }

    private static string ResolveStreamFileNameFromDto(SymbolsEntityDto dto, SymbolsRequest req)
    {
        bool exactPath = dto.IndexPrefix.Equals(req.IndexPrefix, StringComparison.OrdinalIgnoreCase) &&
                         dto.FileName.Equals(req.FileName, StringComparison.OrdinalIgnoreCase);
        if (exactPath)
        {
            return dto.UpstreamFileName ?? dto.FileName;
        }

        return req.FileName;
    }

    /// <summary>
    ///     Stores a symbol (or its not-found marker) in memory cache to reduce database load.
    ///     Positive entries expire after 12 hours; negative entries expire after the remaining upstream recheck window.
    /// </summary>
    /// <param name="request">The symbol request used as the cache key.</param>
    /// <param name="entity">The symbol entity to cache.</param>
    /// <param name="data">Optional blob data; when provided, it is included in the cached DTO.</param>
    private void CacheSymbolInMemory(SymbolsRequest request, SymbolsEntity entity, byte[]? data = null)
    {
        if (data is { Length: > MaxMemoryCachedBlobBytes })
        {
            return;
        }

        SymbolsEntityDto memCacheItem = Mapper.SymbolsEntityToSymbolsEntityDto(entity);

        long entrySize = 1;

        if (data is not null)
        {
            memCacheItem.Blob = data;
            entrySize = Math.Max(1, data.Length);
        }

        TimeSpan absoluteExpiration;

        if (memCacheItem.NotFoundAt.HasValue)
        {
            TimeSpan remaining = options.Value.UpstreamRecheckPeriod -
                (DateTime.UtcNow - memCacheItem.NotFoundAt.Value);
            absoluteExpiration = remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
        else
        {
            absoluteExpiration = PositiveCacheLifetime;
        }

        if (absoluteExpiration > TimeSpan.Zero)
        {
            mc.Set(
                request.ToString(),
                memCacheItem,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiration,
                    Size = entrySize
                }
            );
        }
    }
}
