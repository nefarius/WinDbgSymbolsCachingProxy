using System.Collections.Concurrent;
using System.Net;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

public sealed class RecheckNotFoundService(DB db, IHttpClientFactory clientFactory, ILogger<RecheckNotFoundService> logger)
{
    /// <summary>
    ///     Queries all 404 symbols from DB and contacts the upstream server to check if they have become available since the
    ///     last run. Rechecks symbols previously marked as not found by querying the upstream symbol server and caches any that are now available.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the recheck operation.</param>
    public async Task Run(CancellationToken ct = default)
    {
        List<SymbolsEntity> notFoundSymbols = await db.Find<SymbolsEntity>().ManyAsync(
            sym => sym.NotFoundAt != null && !sym.IsCustom, ct);

        List<SymbolsEntity> customWithAliases = await db.Find<SymbolsEntity>().ManyAsync(
            sym =>
                sym.IsCustom &&
                sym.NotFoundAt == null &&
                sym.AlternateRequestSymbols != null &&
                sym.AlternateRequestSymbols.Count > 0,
            ct);

        HashSet<(string SymbolKey, string Alias)> shadowIndex = new();
        foreach (SymbolsEntity c in customWithAliases)
        {
            string sk = c.SymbolKey.ToLowerInvariant();
            foreach (string alias in c.AlternateRequestSymbols)
            {
                shadowIndex.Add((sk, alias.ToLowerInvariant()));
            }
        }

        logger.LogInformation(
            "RecheckNotFound: {PlaceholderCount} upstream placeholders, {CustomWithAliases} custom rows, {ShadowKeyCount} (symbolKey,alias) shadow keys preloaded",
            notFoundSymbols.Count, customWithAliases.Count, shadowIndex.Count);

        ConcurrentBag<int> shadowRemoved = new();

        // https://stackoverflow.com/a/9290531
        ParallelOptions opts = new()
        {
            CancellationToken = ct,
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75 * 1.0))
        };

        await Parallel.ForEachAsync(notFoundSymbols, opts, async (symbol, innerToken) =>
        {
            if (SymbolAliasLookupService.TryParseSymbolFromIndexPrefix(symbol.IndexPrefix, out string symbolSeg) &&
                !string.IsNullOrEmpty(symbol.SymbolKey) &&
                shadowIndex.Contains((symbol.SymbolKey.ToLowerInvariant(), symbolSeg)))
            {
                logger.LogInformation(
                    "Deleting obsolete upstream not-found row {Symbol} (covered by custom upload alias)",
                    symbol);
                await db.DeleteAsync<SymbolsEntity>(symbol.ID, innerToken);
                shadowRemoved.Add(1);
                return;
            }

            using HttpClient client = clientFactory.CreateClient("MicrosoftSymbolServer");

            using HttpResponseMessage response =
                await client.GetAsync($"download/symbols/{symbol.RelativeUri}", innerToken);

            if (response is { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.NotFound })
            {
                logger.LogInformation("Requested symbol {Symbol} not found upstream", symbol);

                symbol.NotFoundAt = DateTime.UtcNow;
                await db.SaveAsync(symbol, cancellation: innerToken);
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Request failed with unexpected status {StatusCode}", response.StatusCode);
                return;
            }

            if (response.RequestMessage is null)
            {
                logger.LogError("Missing request message");
                return;
            }

            if (response.RequestMessage.RequestUri is null)
            {
                logger.LogError("Missing request URI");
                return;
            }

            string upstreamFilename = Path.GetFileName(response.RequestMessage.RequestUri.AbsolutePath);

            if (string.IsNullOrEmpty(upstreamFilename))
            {
                logger.LogWarning("Failed to extract upstream filename");
                return;
            }

            logger.LogInformation("Got requested symbol {Symbol} ({Filename}), caching",
                symbol, upstreamFilename);

            await using Stream upstreamContent = await response.Content.ReadAsStreamAsync(innerToken);

            await symbol.Data(db).UploadAsync(upstreamContent, cancellation: innerToken);
            symbol.NotFoundAt = null;
            await db.SaveAsync(symbol, cancellation: innerToken);

            logger.LogInformation("Symbol {Symbol} ({Filename}) cached",
                symbol, upstreamFilename);
        });

        logger.LogInformation(
            "RecheckNotFound finished: removed {ShadowRemoved} placeholders as alias-shadowed without upstream fetch",
            shadowRemoved.Count);
    }
}
