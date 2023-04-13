using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

public sealed class RecheckNotFoundService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<RecheckNotFoundService> _logger;

    public RecheckNotFoundService(IHttpClientFactory clientFactory, ILogger<RecheckNotFoundService> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public async Task Run(CancellationToken ct = default)
    {
        List<SymbolsEntity>? notFoundSymbols = await DB.Find<SymbolsEntity>().ManyAsync(
            sym => sym.NotFoundAt != null, ct);

        HttpClient client = _clientFactory.CreateClient("MicrosoftSymbolServer");

        foreach (SymbolsEntity symbol in notFoundSymbols)
        {
            HttpResponseMessage response =
                await client.GetAsync($"download/symbols/{symbol.Symbol}/{symbol.Hash}/{symbol.File}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Requested symbol {@Symbol} not found upstream", symbol);

                symbol.NotFoundAt = DateTime.UtcNow;
                await symbol.SaveAsync(cancellation: ct);
                continue;
            }

            if (response.RequestMessage is null)
            {
                _logger.LogError("Missing request message");
                continue;
            }

            if (response.RequestMessage.RequestUri is null)
            {
                _logger.LogError("Missing request URI");
                continue;
            }

            string upstreamFilename = Path.GetFileName(response.RequestMessage.RequestUri.AbsolutePath);

            if (string.IsNullOrEmpty(upstreamFilename))
            {
                _logger.LogWarning("Failed to extract upstream filename");
                continue;
            }

            _logger.LogInformation("Got requested symbol {@Symbol} ({Filename}), caching",
                symbol, upstreamFilename);

            Stream upstreamContent = await response.Content.ReadAsStreamAsync(ct);

            symbol.NotFoundAt = null;
            await symbol.SaveAsync(cancellation: ct);
            await symbol.Data.UploadAsync(upstreamContent, cancellation: ct);

            _logger.LogInformation("Symbol {@Symbol} ({Filename}) cached",
                symbol, upstreamFilename);
        }
    }
}