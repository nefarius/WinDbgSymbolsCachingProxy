using System.Diagnostics;

using FastEndpoints;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Mappers;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Serves symbol downloads, mirroring the behavior of the Microsoft Symbol Server.
/// </summary>
public sealed class SymbolsDownloadEndpoint(
    ILogger<SymbolsDownloadEndpoint> logger,
    IHttpClientFactory clientFactory,
    IOptions<ServiceConfig> options,
    IMemoryCache mc)
    : Endpoint<SymbolsRequest>
{
    private readonly ActivitySource _activitySource = new(TracingSources.AppActivitySourceName);

    public override void Configure()
    {
        Get("/download/symbols/{Symbol}/{SymbolKey}/{FileName}");
        AllowAnonymous();
        Options(x => x.WithTags("Symbols"));
    }

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

            if (cachedEntity.NotFoundAt.HasValue)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            logger.LogInformation("Returning memory-cached copy for {Entity}", cachedEntity.ToString());

            await Send.BytesAsync(cachedEntity.Blob, cachedEntity.UpstreamFileName ?? cachedEntity.FileName,
                cancellation: ct);
            return;
        }

        Activity? querySymbolInDbActivity = _activitySource.StartActivity(nameof(DB.Find));

        SymbolsEntity? existingSymbol = (await DB.Find<SymbolsEntity>()
                .ManyAsync(lr =>
                        lr.Eq(r => r.IndexPrefix, req.IndexPrefix.ToLowerInvariant()) &
                        lr.Eq(r => r.FileName, req.FileName.ToLowerInvariant())
                    , ct)
            ).FirstOrDefault();

        querySymbolInDbActivity?.Dispose();

        // cached entry found
        if (existingSymbol is not null)
        {
            logger.LogInformation("Found cached symbol {@Symbol}", existingSymbol.ToString());

            // has been checked for existence recently so skip check and return immediately 
            if (existingSymbol.NotFoundAt.HasValue &&
                existingSymbol.NotFoundAt.Value.Add(options.Value.UpstreamRecheckPeriod) > DateTime.UtcNow)
            {
                logger.LogInformation("Cached symbol marked as not found");

                CacheSymbolInMemory(req, existingSymbol);

                await Send.NotFoundAsync(ct);
                return;
            }

            logger.LogInformation("Returning cached copy");

            // deliver cached copy of symbol blob
            using MemoryStream ms = new();

            try
            {
                await existingSymbol.Data.DownloadAsync(ms, cancellation: ct);

                ms.Position = 0;
                await Send.StreamAsync(ms, existingSymbol.UpstreamFileName ?? existingSymbol.FileName,
                    cancellation: ct);

                // update statistics
                existingSymbol.LastAccessedAt = DateTime.UtcNow;
                existingSymbol.AccessedCount = ++existingSymbol.AccessedCount ?? 1;

                CacheSymbolInMemory(req, existingSymbol, ms);

                await existingSymbol.SaveAsync(cancellation: ct);

                return;
            }
            catch (InvalidOperationException)
            {
                logger.LogWarning("Failed to fetch cached copy, re-downloading from upstream");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch cached copy, re-downloading from upstream");
            }
        }

        HttpClient client = clientFactory.CreateClient("MicrosoftSymbolServer");

        HttpResponseMessage response =
            await client.GetAsync($"download/symbols/{req.Symbol}/{req.SymbolKey}/{req.FileName}", ct);

        SymbolsEntity newSymbol = existingSymbol ?? new SymbolsEntity
        {
            CreatedAt = DateTime.UtcNow,
            SymbolKey = req.SymbolKey.ToLowerInvariant(),
            FileName = req.FileName.ToLowerInvariant(),
            IndexPrefix = req.IndexPrefix.ToLowerInvariant()
        };

        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Requested symbol {Symbol} not found upstream", req.ToString());

            // set last 404-timestamp
            newSymbol.NotFoundAt = DateTime.UtcNow;
            await newSymbol.SaveAsync(cancellation: ct);
            CacheSymbolInMemory(req, newSymbol);
            await Send.NotFoundAsync(ct);
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

        logger.LogInformation("Got requested symbol {@Symbol} ({Filename}), caching", req.ToString(), upstreamFilename);

        Stream upstreamContent = await response.Content.ReadAsStreamAsync(ct);

        // cache in memory because we need to return it to database AND requester
        using MemoryStream cache = new();
        await upstreamContent.CopyToAsync(cache, ct);
        cache.Position = 0;

        newSymbol.UpstreamFileName = upstreamFilename;
        newSymbol.NotFoundAt = null;
        newSymbol.LastAccessedAt = DateTime.UtcNow;
        newSymbol.AccessedCount = 1;

        // save and upload to DB
        await newSymbol.SaveAsync(cancellation: ct);
        await newSymbol.Data.UploadAsync(cache, cancellation: ct);

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

            // Log the actual exception regardless
            logger.LogError(ex, "Failed to stream symbol file for {RequestPath}", HttpContext.Request.Path);
        }
    }

    private void CacheSymbolInMemory(SymbolsRequest request, SymbolsEntity entity, MemoryStream? data = null)
    {
        SymbolsEntityMapper mapper = new();

        SymbolsEntityDto memCacheItem = mapper.SymbolsEntityToSymbolsEntityDto(entity);

        if (data is not null)
        {
            data.Position = 0;
            memCacheItem.Blob = data.ToArray();
        }

        mc.Set(
            request.ToString(),
            memCacheItem,
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) }
        );
    }
}