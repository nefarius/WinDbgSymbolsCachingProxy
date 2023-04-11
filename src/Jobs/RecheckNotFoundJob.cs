using Coravel.Invocable;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Jobs;

/// <summary>
///     Periodic job checking upstream for every cached nonexistent symbol entry.
/// </summary>
public sealed class RecheckNotFoundJob : IInvocable
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<RecheckNotFoundJob> _logger;

    public RecheckNotFoundJob(ILogger<RecheckNotFoundJob> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public async Task Invoke()
    {
        _logger.LogInformation("Running 404 re-check task");

        List<SymbolsEntity>? notFoundSymbols = await DB.Find<SymbolsEntity>().ManyAsync(
            sym => sym.NotFoundAt != null
        );

        HttpClient client = _clientFactory.CreateClient("MicrosoftSymbolServer");

        foreach (SymbolsEntity symbol in notFoundSymbols)
        {
            HttpResponseMessage response =
                await client.GetAsync($"download/symbols/{symbol.Symbol}/{symbol.Hash}/{symbol.File}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Requested symbol {@Symbol} not found upstream", symbol);

                symbol.NotFoundAt = DateTime.UtcNow;
                await symbol.SaveAsync();
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

            Stream upstreamContent = await response.Content.ReadAsStreamAsync();

            symbol.NotFoundAt = null;
            await symbol.SaveAsync();
            await symbol.Data.UploadAsync(upstreamContent);

            _logger.LogInformation("Symbol {@Symbol} ({Filename}) cached",
                symbol, upstreamFilename);
        }

        _logger.LogInformation("Re-check task finished");
    }
}