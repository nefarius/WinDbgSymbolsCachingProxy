using System.Collections.Concurrent;
using System.Net;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using MongoDB.Driver;
using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

public sealed class RecheckNotFoundService(
    DB db,
    IHttpClientFactory clientFactory,
    ILogger<RecheckNotFoundService> logger,
    IOptions<ServiceConfig> options,
    IMemoryCache memoryCache)
{
    /// <summary>
    ///     Queries all 404 symbols from DB and contacts the upstream server to check if they have become available since the
    ///     last run. Rechecks symbols previously marked as not found by querying the upstream symbol server and caches any that are now available.
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the recheck operation.</param>
    public async Task Run(CancellationToken ct = default)
    {
        await PruneStaleNotFoundAsync(ct);

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
            if (string.IsNullOrWhiteSpace(c.SymbolKey) || c.AlternateRequestSymbols is null)
            {
                continue;
            }

            string sk = c.SymbolKey.Trim().ToLowerInvariant();
            foreach (string? alias in c.AlternateRequestSymbols)
            {
                string? aliasTrimmed = alias?.Trim();
                if (string.IsNullOrEmpty(aliasTrimmed))
                {
                    continue;
                }

                shadowIndex.Add((sk, aliasTrimmed.ToLowerInvariant()));
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

        DateTime now = DateTime.UtcNow;
        // Honor the configured recheck period instead of hitting upstream for every placeholder daily.
        notFoundSymbols = notFoundSymbols
            .Where(s => LastActivity(s) + options.Value.UpstreamRecheckPeriod <= now)
            .ToList();

        await Parallel.ForEachAsync(notFoundSymbols, opts, async (symbol, innerToken) =>
        {
            try
            {
                await RecheckOneAsync(symbol, shadowIndex, shadowRemoved, innerToken);
            }
            catch (OperationCanceledException) when (innerToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RecheckNotFound failed for {Symbol}; continuing with remaining symbols", symbol);
            }
        });

        logger.LogInformation(
            "RecheckNotFound finished: removed {ShadowRemoved} placeholders as alias-shadowed without upstream fetch",
            shadowRemoved.Count);
    }

    /// <summary>
    ///     The later of the last upstream check and the first not-found time; a missing timestamp counts as absent.
    /// </summary>
    private static readonly TimeSpan UpstreamBodyTimeout = TimeSpan.FromMinutes(10);

    private static DateTime LastActivity(SymbolsEntity symbol)
    {
        DateTime last = DateTime.MinValue;
        if (symbol.LastCheckedAt is { } checkedAt && checkedAt > last)
        {
            last = checkedAt;
        }

        if (symbol.NotFoundAt is { } notFoundAt && notFoundAt > last)
        {
            last = notFoundAt;
        }

        return last;
    }

    private async Task RecheckOneAsync(
        SymbolsEntity symbol,
        HashSet<(string SymbolKey, string Alias)> shadowIndex,
        ConcurrentBag<int> shadowRemoved,
        CancellationToken innerToken)
    {
        {
            if (SymbolAliasLookupService.TryParseSymbolFromIndexPrefix(symbol.IndexPrefix, out string symbolSeg) &&
                !string.IsNullOrEmpty(symbol.SymbolKey))
            {
                string normalizedSymbolSeg = symbolSeg.ToLowerInvariant();
                if (shadowIndex.Contains((symbol.SymbolKey.ToLowerInvariant(), normalizedSymbolSeg)))
                {
                    logger.LogInformation(
                        "Deleting obsolete upstream not-found row {Symbol} (covered by custom upload alias)",
                        symbol);
                    await db.DeleteAsync<SymbolsEntity>(symbol.ID, CancellationToken.None);
                    shadowRemoved.Add(1);
                    return;
                }
            }

            HttpClient client = clientFactory.CreateClient("MicrosoftSymbolServer");

            using HttpResponseMessage response = await client.GetAsync(
                $"download/symbols/{symbol.RelativeUri}", HttpCompletionOption.ResponseHeadersRead, innerToken);

            if (response is { IsSuccessStatusCode: false, StatusCode: HttpStatusCode.NotFound or HttpStatusCode.Gone })
            {
                logger.LogInformation("Requested symbol {Symbol} not found upstream", symbol);

                // NotFoundAt intentionally stays untouched so cleanup can measure how long the symbol has been missing.
                await db.Update<SymbolsEntity>()
                    .MatchID(symbol.ID)
                    .Modify(b => b.Set(e => e.LastCheckedAt, DateTime.UtcNow))
                    .ExecuteAsync(innerToken);
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

            // ResponseHeadersRead leaves the body unbounded by the resilience pipeline: bound the read, then upload from
            // a temporary file (disk, not memory) so the upload does not depend on upstream read cancellation.
            using CancellationTokenSource bodyTimeout = CancellationTokenSource.CreateLinkedTokenSource(innerToken);
            bodyTimeout.CancelAfter(UpstreamBodyTimeout);

            string tempPath = Path.Combine(Path.GetTempPath(), $"wdscp-recheck-{Guid.NewGuid():N}.tmp");
            await using FileStream buffer = new(tempPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 81920,
                FileOptions.Asynchronous | FileOptions.DeleteOnClose);
            try
            {
                await using Stream upstreamContent = await response.Content.ReadAsStreamAsync(bodyTimeout.Token);
                await upstreamContent.CopyToAsync(buffer, bodyTimeout.Token);
            }
            catch (OperationCanceledException) when (!innerToken.IsCancellationRequested)
            {
                logger.LogWarning("Reading upstream symbol {Symbol} timed out after {Timeout}", symbol, UpstreamBodyTimeout);
                return;
            }

            buffer.Position = 0;
            await symbol.Data(db).UploadAsync(buffer, cancellation: innerToken);
            // Targeted update: saving the whole loaded entity could overwrite concurrent access-count/timestamp updates.
            await db.Update<SymbolsEntity>()
                .MatchID(symbol.ID)
                .Modify(b => b
                    .Set(e => e.NotFoundAt, (DateTime?)null)
                    .Set(e => e.LastCheckedAt, (DateTime?)null)
                    .Set(e => e.UpstreamFileName, upstreamFilename))
                .ExecuteAsync(innerToken);

            // drop the cached 404 so clients see the symbol right away
            memoryCache.Remove(symbol.RelativeUri);

            logger.LogInformation("Symbol {Symbol} ({Filename}) cached",
                symbol, upstreamFilename);
        }
    }

    /// <summary>
    ///     Deletes stale not-found placeholders from the database and evicts them from the memory cache.
    ///     Two rules apply (both only to upstream, non-custom rows):
    ///     <list type="bullet">
    ///         <item>
    ///             <b>Rule A (inactive):</b> the symbol was requested at some point but
    ///             <see cref="SymbolsEntity.LastAccessedAt" /> is older than <see cref="NotFoundCleanupOptions.InactiveAfter" />.
    ///         </item>
    ///         <item>
    ///             <b>Rule B (unused):</b> the symbol was <em>never</em> requested by any client
    ///             (<see cref="SymbolsEntity.LastAccessedAt" /> is <see langword="null" />) and
    ///             <see cref="SymbolsEntity.NotFoundAt" /> is older than <see cref="NotFoundCleanupOptions.UnusedNotFoundAfter" />.
    ///         </item>
    ///     </list>
    /// </summary>
    private async Task PruneStaleNotFoundAsync(CancellationToken ct)
    {
        NotFoundCleanupOptions cleanup = options.Value.NotFoundCleanup;

        if (!cleanup.Enabled)
        {
            return;
        }

        if (cleanup.InactiveAfter <= TimeSpan.Zero || cleanup.UnusedNotFoundAfter <= TimeSpan.Zero)
        {
            logger.LogWarning(
                "NotFoundCleanup: skipping pruning because {InactiveAfter} or {UnusedNotFoundAfter} is zero or negative — set both to a positive TimeSpan to enable cleanup",
                cleanup.InactiveAfter, cleanup.UnusedNotFoundAfter);
            return;
        }

        DateTime inactiveCutoff = DateTime.UtcNow - cleanup.InactiveAfter;
        DateTime unusedCutoff = DateTime.UtcNow - cleanup.UnusedNotFoundAfter;

        FilterDefinitionBuilder<SymbolsEntity> fb = Builders<SymbolsEntity>.Filter;

        FilterDefinition<SymbolsEntity> filter = fb.And(
            fb.Ne(s => s.IsCustom, true),
            fb.Ne(s => s.NotFoundAt, null),
            fb.Or(
                // Rule A: had a client, but not recently
                fb.And(
                    fb.Ne(s => s.LastAccessedAt, null),
                    fb.Lt(s => s.LastAccessedAt, inactiveCutoff)),
                // Rule B: never had a client, 404 for too long
                fb.And(
                    fb.Eq(s => s.LastAccessedAt, (DateTime?)null),
                    fb.Lt(s => s.NotFoundAt, unusedCutoff))));

        List<SymbolsEntity> stale = await db.Find<SymbolsEntity>().Match(_ => filter).ExecuteAsync(ct);

        if (stale.Count == 0)
        {
            logger.LogInformation("NotFoundCleanup: no stale placeholders found");
            return;
        }

        int inactiveCount = stale.Count(s => s.LastAccessedAt is not null);
        int unusedCount = stale.Count - inactiveCount;

        foreach (SymbolsEntity s in stale)
        {
            memoryCache.Remove(s.RelativeUri);
        }

        IEnumerable<object> ids = stale.Select(s => (object)s.ID);
        await db.DeleteAsync<SymbolsEntity>(ids, ct);

        logger.LogInformation(
            "NotFoundCleanup: removed {InactiveCount} inactive, {UnusedCount} unused placeholders",
            inactiveCount, unusedCount);
    }

    /// <summary>
    ///     Returns <see langword="true" /> when <paramref name="entity" /> matches the stale not-found cleanup predicate.
    ///     This is the in-process equivalent of the MongoDB filter used in <see cref="PruneStaleNotFoundAsync" /> and is
    ///     exposed as <see langword="internal" /> to allow unit-testing the rules without a live database.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <param name="inactiveCutoff">
    ///     Entities with a <see cref="SymbolsEntity.LastAccessedAt" /> strictly before this value match Rule A.
    /// </param>
    /// <param name="unusedCutoff">
    ///     Entities with no <see cref="SymbolsEntity.LastAccessedAt" /> and a <see cref="SymbolsEntity.NotFoundAt" />
    ///     strictly before this value match Rule B.
    /// </param>
    internal static bool IsStaleNotFound(SymbolsEntity entity, DateTime inactiveCutoff, DateTime unusedCutoff)
    {
        if (entity.IsCustom || entity.NotFoundAt is null)
        {
            return false;
        }

        // Rule A: was requested before but not recently
        if (entity.LastAccessedAt is not null)
        {
            return entity.LastAccessedAt < inactiveCutoff;
        }

        // Rule B: never requested, 404 for too long
        return entity.NotFoundAt < unusedCutoff;
    }
}
