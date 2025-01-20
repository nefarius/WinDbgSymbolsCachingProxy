using FastEndpoints;

using Microsoft.Extensions.Options;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Serves symbol downloads, mirroring the behavior of the Microsoft Symbol Server.
/// </summary>
public sealed class SymbolsDownloadEndpoint : Endpoint<SymbolsRequest>
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<SymbolsDownloadEndpoint> _logger;
    private readonly IOptions<ServiceConfig> _options;

    public SymbolsDownloadEndpoint(ILogger<SymbolsDownloadEndpoint> logger, IHttpClientFactory clientFactory,
        IOptions<ServiceConfig> options)
    {
        _logger = logger;
        _clientFactory = clientFactory;
        _options = options;
    }

    public override void Configure()
    {
        Get("/download/symbols/{Symbol}/{SymbolKey}/{FileName}");
        AllowAnonymous();
        Options(x => x.WithTags("Symbols"));
    }

    public override async Task HandleAsync(SymbolsRequest req, CancellationToken ct)
    {
        SymbolsEntity? existingSymbol = (await DB.Find<SymbolsEntity>()
                .ManyAsync(lr =>
                        lr.Eq(r => r.IndexPrefix, req.IndexPrefix.ToLowerInvariant()) &
                        lr.Eq(r => r.FileName, req.FileName.ToLowerInvariant())
                    , ct)
            ).FirstOrDefault();

        // cached entry found
        if (existingSymbol is not null)
        {
            _logger.LogInformation("Found cached symbol {@Symbol}", existingSymbol);

            // has been checked for existence recently so skip check and return immediately 
            if (existingSymbol.NotFoundAt.HasValue &&
                existingSymbol.NotFoundAt.Value.Add(_options.Value.UpstreamRecheckPeriod) > DateTime.UtcNow)
            {
                _logger.LogInformation("Cached symbol marked as not found");

                await SendNotFoundAsync(ct);
                return;
            }

            _logger.LogInformation("Returning cached copy");

            // deliver cached copy of symbol blob
            using MemoryStream ms = new();

            try
            {
                await existingSymbol.Data.DownloadAsync(ms, cancellation: ct);

                ms.Position = 0;
                await SendStreamAsync(ms, existingSymbol.UpstreamFileName ?? existingSymbol.FileName, cancellation: ct);

                // update statistics
                existingSymbol.LastAccessedAt = DateTime.UtcNow;
                existingSymbol.AccessedCount = ++existingSymbol.AccessedCount ?? 1;

                await existingSymbol.SaveAsync(cancellation: ct);

                return;
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning("Failed to fetch cached copy, re-downloading from upstream");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch cached copy, re-downloading from upstream");
            }
        }

        HttpClient client = _clientFactory.CreateClient("MicrosoftSymbolServer");

        HttpResponseMessage response =
            await client.GetAsync($"download/symbols/{req.Symbol}/{req.SymbolKey}/{req.FileName}", ct);

        SymbolsEntity newSymbol = existingSymbol ?? new SymbolsEntity
        {
            SymbolKey = req.SymbolKey.ToLowerInvariant(),
            FileName = req.FileName.ToLowerInvariant(),
            IndexPrefix = req.IndexPrefix.ToLowerInvariant()
        };

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Requested symbol {@Symbol} not found upstream", req);

            // set last 404 timestamp
            newSymbol.NotFoundAt = DateTime.UtcNow;
            await newSymbol.SaveAsync(cancellation: ct);
            await SendNotFoundAsync(ct);
            return;
        }

        if (response.RequestMessage is null)
        {
            _logger.LogError("Missing request message");
            AddError("Missing request message");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (response.RequestMessage.RequestUri is null)
        {
            _logger.LogError("Missing request URI");
            AddError("Missing request URI");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        // grab the filename we got after 302 got resolved
        string upstreamFilename = Path.GetFileName(response.RequestMessage.RequestUri.AbsolutePath);

        if (string.IsNullOrEmpty(upstreamFilename))
        {
            _logger.LogWarning("Failed to extract upstream filename");

            // fallback value
            upstreamFilename = req.FileName.ToLowerInvariant();
        }

        _logger.LogInformation("Got requested symbol {@Symbol} ({Filename}), caching", req, upstreamFilename);

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

        cache.Position = 0;

        await SendStreamAsync(cache, upstreamFilename, cancellation: ct);
    }
}