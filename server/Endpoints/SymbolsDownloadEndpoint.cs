using System.Diagnostics;
using System.Net;

using FastEndpoints;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using MongoDB.Driver;
using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Mappers;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Serves symbol downloads, mirroring the behavior of the Microsoft Symbol Server.
/// </summary>
public sealed class SymbolsDownloadEndpoint(
    DB db,
    ILogger<SymbolsDownloadEndpoint> logger,
    IHttpClientFactory clientFactory,
    IOptions<ServiceConfig> options,
    IMemoryCache mc)
    : Endpoint<SymbolsRequest>
{
    private readonly ActivitySource _activitySource = new(TracingSources.AppActivitySourceName);

    /// <summary>
    ///     Configures the GET route for symbol download at "/download/symbols/{Symbol}/{SymbolKey}/{FileName}" with anonymous access and the "Symbols" tag.
    /// </summary>
    public override void Configure()
    {
        Get("/download/symbols/{Symbol}/{SymbolKey}/{FileName}");
        AllowAnonymous();
        Options(x => x.WithTags("Symbols"));
    }

    /// <summary>
    ///     Processes the request for downloading and caching a symbol file. The method attempts to retrieve the symbol from
    ///     memory cache, database, or an upstream symbol server.
    ///     It ensures the symbol is properly cached for future requests and streams the file to the client.
    /// </summary>
    /// <param name="req">The symbol request containing details such as IndexPrefix, Symbol, SymbolKey, and FileName.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation of handling the symbol request.</returns>
    public override async Task HandleAsync(SymbolsRequest req, CancellationToken ct)
    {
        HttpContext.RequestAborted.Register(() =>
        {
            logger.LogWarning("Client disconnected while streaming file: {RequestPath}", HttpContext.Request.Path);
        });

        Activity? parentActivity = Activity.Current;

        parentActivity?.AddTag("request.IndexPrefix", req.IndexPrefix);
        parentActivity?.AddTag("request.FileName", req.FileName);

        // try to probe memory cache before querying DB
        if (mc.TryGetValue(req.ToString(), out SymbolsEntityDto? cachedEntity) && cachedEntity is not null)
        {
            logger.LogInformation("Found cached copy in memory for {Request}", req.ToString());

            if (cachedEntity.NotFoundAt.HasValue || cachedEntity.Blob is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            logger.LogInformation("Returning memory-cached copy for {Entity}", cachedEntity.ToString());

            await Send.BytesAsync(cachedEntity.Blob, cachedEntity.UpstreamFileName ?? cachedEntity.FileName,
                cancellation: ct);

            SymbolsEntity? entityToUpdate = (await db.Find<SymbolsEntity>()
                    .ManyAsync(lr =>
                            lr.Eq(r => r.IndexPrefix, req.IndexPrefix.ToLowerInvariant()) &
                            lr.Eq(r => r.FileName, req.FileName.ToLowerInvariant())
                        , CancellationToken.None))
                .FirstOrDefault();
            if (entityToUpdate is not null)
            {
                entityToUpdate.LastAccessedAt = DateTime.UtcNow;
                entityToUpdate.AccessedCount = (entityToUpdate.AccessedCount ?? 0) + 1;
                await db.SaveAsync(entityToUpdate, CancellationToken.None);
            }
            return;
        }

        Activity? querySymbolInDbActivity = _activitySource.StartActivity("Find");

        SymbolsEntity? existingSymbol = (await db.Find<SymbolsEntity>()
                .ManyAsync(lr =>
                        lr.Eq(r => r.IndexPrefix, req.IndexPrefix.ToLowerInvariant()) &
                        lr.Eq(r => r.FileName, req.FileName.ToLowerInvariant())
                    , CancellationToken.None)
            ).FirstOrDefault();

        querySymbolInDbActivity?.Dispose();

        // cached entry found
        if (existingSymbol is not null)
        {
            logger.LogInformation("Found cached symbol {@Symbol}", existingSymbol.ToString());

            if (existingSymbol.NotFoundAt.HasValue)
            {
                if (existingSymbol.NotFoundAt.Value.Add(options.Value.UpstreamRecheckPeriod) > DateTime.UtcNow)
                {
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
                    ms.Position = 0;
                    byte[] blob = ms.ToArray();
                    existingSymbol.LastAccessedAt = DateTime.UtcNow;
                    existingSymbol.AccessedCount = (existingSymbol.AccessedCount ?? 0) + 1;
                    CacheSymbolInMemory(req, existingSymbol, new MemoryStream(blob));

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await db.SaveAsync(existingSymbol, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Failed to persist access metadata for {IndexPrefix}", existingSymbol.IndexPrefix);
                        }
                    });

                    try
                    {
                        logger.LogInformation("Returning cached copy");
                        await Send.StreamAsync(new MemoryStream(blob),
                            existingSymbol.UpstreamFileName ?? existingSymbol.FileName,
                            cancellation: ct);
                    }
                    catch (Exception ex)
                    {
                        if (HttpContext.Response.HasStarted)
                        {
                            logger.LogWarning(ex,
                                "Error after starting response (e.g. client disconnect), cache entry unchanged");
                        }
                        else
                        {
                            logger.LogWarning(ex, "Error while streaming cached symbol to client");
                            HttpContext.Response.StatusCode = 500;
                            await HttpContext.Response.WriteAsync("Error while streaming cached symbol to client.", ct);
                            return;
                        }
                    }

                    return;
                }
            }
        }

        HttpClient client = clientFactory.CreateClient("MicrosoftSymbolServer");

        HttpResponseMessage response;

        try
        {
            response = await client.GetAsync(
                $"download/symbols/{req.Symbol}/{req.SymbolKey}/{req.FileName}",
                HttpCompletionOption.ResponseHeadersRead,
                ct);
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Client disconnected during upstream fetch for {Request}", req.ToString());
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

                    if (existingSymbol is null)
                    {
                        newSymbol.NotFoundAt = DateTime.UtcNow;
                        await db.SaveAsync(newSymbol, CancellationToken.None);
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

            if (response.RequestMessage is null)
            {
                logger.LogError("Missing request message");
                AddError("Missing request message");
                await Send.ErrorsAsync(cancellation: ct);
                return;
            }

            if (response.RequestMessage.RequestUri is null)
            {
                logger.LogError("Missing request URI");
                AddError("Missing request URI");
                await Send.ErrorsAsync(cancellation: ct);
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

            logger.LogInformation("Got requested symbol {@Symbol} ({Filename}), caching", req.ToString(),
                upstreamFilename);

            await using Stream upstreamContent =
                await response.Content.ReadAsStreamAsync(CancellationToken.None);

            // cache in memory because we need to return it to database AND requester
            using MemoryStream cache = new();
            await upstreamContent.CopyToAsync(cache, CancellationToken.None);
            cache.Position = 0;

            newSymbol.UpstreamFileName = upstreamFilename;
            newSymbol.NotFoundAt = DateTime.MinValue;

            await db.SaveAsync(newSymbol, CancellationToken.None);

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
            CacheSymbolInMemory(req, newSymbol, cache);

            cache.Position = 0;

            try
            {
                await Send.StreamAsync(cache, upstreamFilename, cancellation: ct);
            }
            catch (Exception ex)
            {
                if (!HttpContext.Response.HasStarted)
                {
                    HttpContext.Response.StatusCode = 500;
                    await HttpContext.Response.WriteAsync("Error while sending symbol file.", ct);
                }

                logger.LogError(ex, "Failed to stream symbol file for {RequestPath}", HttpContext.Request.Path);
            }
        }
        finally
        {
            response.Dispose();
        }
    }

    /// <summary>
    ///     Stores a symbol (or its not-found marker) in memory cache to reduce database load.
    ///     Positive entries expire after 12 hours; negative entries expire after the remaining upstream recheck window.
    /// </summary>
    /// <param name="request">The symbol request used as the cache key.</param>
    /// <param name="entity">The symbol entity to cache.</param>
    /// <param name="data">Optional blob data; when provided, it is included in the cached DTO.</param>
    private void CacheSymbolInMemory(SymbolsRequest request, SymbolsEntity entity, MemoryStream? data = null)
    {
        SymbolsEntityMapper mapper = new();

        SymbolsEntityDto memCacheItem = mapper.SymbolsEntityToSymbolsEntityDto(entity);

        long entrySize = 1;

        if (data is not null)
        {
            data.Position = 0;
            memCacheItem.Blob = data.ToArray();
            entrySize = memCacheItem.Blob.Length;
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
            absoluteExpiration = TimeSpan.FromHours(12);
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