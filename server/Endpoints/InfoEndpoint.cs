using FastEndpoints;

using Microsoft.Extensions.Caching.Memory;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class InfoEndpoint(DB db, ILogger<InfoEndpoint> logger, IMemoryCache memoryCache)
    : EndpointWithoutRequest<RootResponse>
{
    /// <summary>
    /// Configures routing and metadata for the "/info" endpoint.
    /// </summary>
    /// <remarks>
    /// Maps the endpoint to HTTP GET at path "/info", allows anonymous access, and excludes the endpoint from OpenAPI/endpoint descriptions.
    /// </remarks>
    public override void Configure()
    {
        Get("/info");
        AllowAnonymous();
        Options(builder => builder.ExcludeFromDescription());
    }

    /// <summary>
    /// Handles the /info request by sending a RootResponse containing the server version and cached symbol counts.
    /// </summary>
    /// <remarks>
    /// If an in-memory cached response exists it is returned. Otherwise the endpoint resolves the server version from assembly metadata (and PE resources as a fallback), stores the response in memory for one hour, and sends it. If no version string is available, counts are still returned and a warning is logged.
    /// </remarks>
    public override async Task HandleAsync(CancellationToken ct)
    {
        if (memoryCache.TryGetValue(nameof(InfoEndpoint), out RootResponse? response) && response is not null)
        {
            await Send.OkAsync(response, ct);
            return;
        }

        string? serverVersion = ApplicationVersionHelper.TryGetServerVersion();

        if (serverVersion is null)
            logger.LogWarning("Could not determine server version; returning counts only");

        response = new RootResponse
        {
            ServerVersion = serverVersion,
            CachedSymbolsTotal = await db.CountAsync<SymbolsEntity>(cancellation: ct),
            CachedSymbols404 = await db.CountAsync<SymbolsEntity>(
                s => s.NotFoundAt != null,
                cancellation: ct),
            CachedSymbolsFound = await db.CountAsync<SymbolsEntity>(
                s => s.NotFoundAt == null,
                cancellation: ct)
        };

        memoryCache.Set(nameof(InfoEndpoint), response, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            Size = 1
        });

        await Send.OkAsync(response, ct);
    }
}