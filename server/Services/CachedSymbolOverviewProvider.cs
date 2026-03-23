using Microsoft.Extensions.Caching.Memory;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Provides <see cref="RootResponse" /> with in-memory caching (shared by /info and OG image).
/// </summary>
public interface ICachedSymbolOverviewProvider
{
    /// <summary>
    ///     Returns cached or freshly loaded symbol overview statistics.
    /// </summary>
    Task<RootResponse> GetAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class CachedSymbolOverviewProvider(
    DB db,
    IMemoryCache memoryCache,
    ILogger<CachedSymbolOverviewProvider> logger) : ICachedSymbolOverviewProvider
{
    internal const string CacheKey = nameof(CachedSymbolOverviewProvider);

    /// <inheritdoc />
    public async Task<RootResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue(CacheKey, out RootResponse? cached) && cached is not null)
            return cached;

        string? serverVersion = ApplicationVersionHelper.TryGetServerVersion();

        if (serverVersion is null)
            logger.LogWarning("Could not determine server version; returning counts only");

        RootResponse response = new()
        {
            ServerVersion = serverVersion,
            CachedSymbolsTotal = await db.CountAsync<SymbolsEntity>(cancellation: cancellationToken),
            CachedSymbols404 = await db.CountAsync<SymbolsEntity>(
                s => s.NotFoundAt != null,
                cancellation: cancellationToken),
            CachedSymbolsFound = await db.CountAsync<SymbolsEntity>(
                s => s.NotFoundAt == null,
                cancellation: cancellationToken)
        };

        memoryCache.Set(CacheKey, response, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            Size = 1
        });

        return response;
    }
}
