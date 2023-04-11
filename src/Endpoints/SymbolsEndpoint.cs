using FastDeepCloner;

using FastEndpoints;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class SymbolsEndpoint : Endpoint<SymbolsRequest>
{
    private static readonly TimeSpan UpstreamRecheckPeriod = TimeSpan.FromDays(7);
    private readonly IHttpClientFactory _clientFactory;

    private readonly ILogger<SymbolsEndpoint> _logger;

    public SymbolsEndpoint(ILogger<SymbolsEndpoint> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public override void Configure()
    {
        Get("/download/symbols/{Symbol}/{Hash}/{File}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SymbolsRequest req, CancellationToken ct)
    {
        SymbolsEntity? existingSymbol = (await DB.Find<SymbolsEntity>()
                .ManyAsync(lr =>
                        lr.Eq(r => r.Symbol, req.Symbol) &
                        lr.Eq(r => r.Hash, req.Hash) &
                        lr.Eq(r => r.File, req.File)
                    , ct)
            ).FirstOrDefault();

        // cached entry found
        if (existingSymbol is not null)
        {
            _logger.LogInformation("Found cached symbol {@Symbol}", existingSymbol);

            // has been checked for existence recently so skip check and return immediately 
            if (existingSymbol.NotFoundAt.HasValue &&
                existingSymbol.NotFoundAt.Value.Add(UpstreamRecheckPeriod) > DateTime.UtcNow)
            {
                _logger.LogInformation("Cached symbol marked as not found");

                await SendNotFoundAsync(ct);
                return;
            }

            _logger.LogInformation("Returning cached copy");

            // deliver cached copy of symbol blob
            using MemoryStream ms = new();
            await existingSymbol.Data.DownloadAsync(ms, cancellation: ct);
            ms.Position = 0;
            await SendStreamAsync(ms, existingSymbol.UpstreamFileName ?? existingSymbol.File, cancellation: ct);
            return;
        }

        HttpClient client = _clientFactory.CreateClient("MicrosoftSymbolServer");

        HttpResponseMessage response =
            await client.GetAsync($"download/symbols/{req.Symbol}/{req.Hash}/{req.File}", ct);

        SymbolsEntity newSymbol = new();
        req.CloneTo(newSymbol);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Requested symbol {@Symbol} not found upstream", req);

            newSymbol.NotFoundAt = DateTime.UtcNow;
            await newSymbol.SaveAsync(cancellation: ct);
            await SendNotFoundAsync(ct);
            return;
        }

        if (response.RequestMessage is null)
        {
            _logger.LogError("Missing request message");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        if (response.RequestMessage.RequestUri is null)
        {
            _logger.LogError("Missing request URI");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        string upstreamFilename = Path.GetFileName(response.RequestMessage.RequestUri.AbsolutePath);

        if (string.IsNullOrEmpty(upstreamFilename))
        {
            _logger.LogWarning("Failed to extract upstream filename");
        }

        _logger.LogInformation("Got requested symbol {@Symbol} ({Filename}), caching", req, upstreamFilename);

        Stream upstreamContent = await response.Content.ReadAsStreamAsync(ct);

        // cache in memory because we need to return it to the requester as well
        using MemoryStream cache = new();
        await upstreamContent.CopyToAsync(cache, ct);
        cache.Position = 0;

        newSymbol.UpstreamFileName = upstreamFilename;
        newSymbol.NotFoundAt = null;
        await newSymbol.SaveAsync(cancellation: ct);
        await newSymbol.Data.UploadAsync(cache, cancellation: ct);

        cache.Position = 0;

        await SendStreamAsync(cache, upstreamFilename, cancellation: ct);
    }
}