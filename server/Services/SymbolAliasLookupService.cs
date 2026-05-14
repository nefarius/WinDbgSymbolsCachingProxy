using MongoDB.Driver;
using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Resolves custom-uploaded symbols when WinDbg uses a distribution name that differs from the embedded canonical name.
/// </summary>
public sealed class SymbolAliasLookupService(DB db)
{
    /// <summary>
    ///     Finds a custom symbol row that has blob data and lists <paramref name="requestedSymbolLower" /> in
    ///     <see cref="SymbolsEntity.AlternateRequestSymbols" /> for the given <paramref name="symbolKeyLower" />.
    /// </summary>
    /// <remarks>
    ///     <paramref name="symbolKeyLower" /> and <paramref name="requestedSymbolLower" /> must already be lowercased
    ///     (e.g. <c>ToLowerInvariant()</c>) so the query matches stored values.
    /// </remarks>
    public async Task<SymbolsEntity?> FindCustomSymbolByRequestPathAsync(string symbolKeyLower,
        string requestedSymbolLower, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(symbolKeyLower) || string.IsNullOrEmpty(requestedSymbolLower))
        {
            return null;
        }

        FilterDefinition<SymbolsEntity> filter = Builders<SymbolsEntity>.Filter.And(
            Builders<SymbolsEntity>.Filter.Eq(e => e.IsCustom, true),
            Builders<SymbolsEntity>.Filter.Eq(e => e.NotFoundAt, null),
            Builders<SymbolsEntity>.Filter.Eq(e => e.SymbolKey, symbolKeyLower),
            Builders<SymbolsEntity>.Filter.AnyEq(e => e.AlternateRequestSymbols, requestedSymbolLower));

        List<SymbolsEntity> matches = await db.Find<SymbolsEntity>().ManyAsync(_ => filter, cancellationToken);

        return PickBest(matches);
    }

    /// <summary>
    ///     Parses the first path segment of <paramref name="indexPrefix" /> (the symbol name before the first '/').
    /// </summary>
    public static bool TryParseSymbolFromIndexPrefix(string indexPrefix, out string symbolSegmentLower)
    {
        symbolSegmentLower = "";
        if (string.IsNullOrEmpty(indexPrefix))
        {
            return false;
        }

        int slash = indexPrefix.IndexOf('/');
        if (slash <= 0)
        {
            return false;
        }

        symbolSegmentLower = indexPrefix[..slash].ToLowerInvariant();
        return true;
    }

    /// <summary>
    ///     Returns whether an upstream not-found placeholder is obsolete because a custom symbol with matching key and alias covers it.
    /// </summary>
    public async Task<bool> IsUpstreamNotFoundShadowedByCustomAliasAsync(SymbolsEntity upstreamNotFoundPlaceholder,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(upstreamNotFoundPlaceholder.SymbolKey))
        {
            return false;
        }

        if (!TryParseSymbolFromIndexPrefix(upstreamNotFoundPlaceholder.IndexPrefix, out string symbolSeg))
        {
            return false;
        }

        SymbolsEntity? custom = await FindCustomSymbolByRequestPathAsync(
            upstreamNotFoundPlaceholder.SymbolKey.ToLowerInvariant(), symbolSeg, cancellationToken);

        return custom is not null;
    }

    internal static SymbolsEntity? PickBest(IReadOnlyList<SymbolsEntity> matches)
    {
        if (matches.Count == 0)
        {
            return null;
        }

        if (matches.Count == 1)
        {
            return matches[0];
        }

        static DateTime TieBreaker(SymbolsEntity e) =>
            e.UploadedAt ?? e.LastAccessedAt ?? e.CreatedAt ?? DateTime.MinValue;

        return matches.OrderByDescending(TieBreaker).First();
    }
}
