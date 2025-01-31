using System.Net;

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

    /// <summary>
    ///     Queries all 404 symbols from DB and contacts the upstream server to check if they have become available since the
    ///     last run.
    /// </summary>
    public async Task Run(CancellationToken ct = default)
    {
        List<SymbolsEntity> notFoundSymbols = await DB.Find<SymbolsEntity>().ManyAsync(
            sym => sym.NotFoundAt != null && !sym.IsCustom, ct);

        // https://stackoverflow.com/a/9290531
        ParallelOptions opts = new()
        {
            CancellationToken = ct,
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 1.0))
        };

        // boost performance by issuing requests in parallel
        await Parallel.ForEachAsync(notFoundSymbols, opts, async (symbol, token) =>
        {
            using HttpClient client = _clientFactory.CreateClient("MicrosoftSymbolServer");

            using HttpResponseMessage response =
                await client.GetAsync($"download/symbols/{symbol.RelativeUri}", token);

            if (response is { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.NotFound })
            {
                _logger.LogInformation("Requested symbol {Symbol} not found upstream", symbol);

                symbol.NotFoundAt = DateTime.UtcNow;
                await symbol.SaveAsync(cancellation: ct);
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Request failed with unexpected status {StatusCode}", response.StatusCode);
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

            _logger.LogInformation("Got requested symbol {Symbol} ({Filename}), caching",
                symbol, upstreamFilename);

            await using Stream upstreamContent = await response.Content.ReadAsStreamAsync(token);

            symbol.NotFoundAt = null;
            await symbol.SaveAsync(cancellation: token);
            await symbol.Data.UploadAsync(upstreamContent, cancellation: token);

            _logger.LogInformation("Symbol {Symbol} ({Filename}) cached",
                symbol, upstreamFilename);
        });
    }
}