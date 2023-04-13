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

        await Parallel.ForEachAsync(notFoundSymbols, ct, async (symbol, token) =>
        {
            HttpResponseMessage response =
                await client.GetAsync($"download/symbols/{symbol.Symbol}/{symbol.Hash}/{symbol.File}", token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Requested symbol {@Symbol} not found upstream", symbol);

                symbol.NotFoundAt = DateTime.UtcNow;
                await symbol.SaveAsync(cancellation: ct);
                return;
            }

            if (response.RequestMessage is null)
            {
                _logger.LogError("Missing request message");
                return;
            }

            if (response.RequestMessage.RequestUri is null)
            {
                _logger.LogError("Missing request URI");
                return;
            }

            string upstreamFilename = Path.GetFileName(response.RequestMessage.RequestUri.AbsolutePath);

            if (string.IsNullOrEmpty(upstreamFilename))
            {
                _logger.LogWarning("Failed to extract upstream filename");
                return;
            }

            _logger.LogInformation("Got requested symbol {@Symbol} ({Filename}), caching",
                symbol, upstreamFilename);

            Stream upstreamContent = await response.Content.ReadAsStreamAsync(token);

            symbol.NotFoundAt = null;
            await symbol.SaveAsync(cancellation: token);
            await symbol.Data.UploadAsync(upstreamContent, cancellation: token);

            _logger.LogInformation("Symbol {@Symbol} ({Filename}) cached",
                symbol, upstreamFilename);
        });
    }
}