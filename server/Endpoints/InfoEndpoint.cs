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
    /// If an in-memory cached response exists it is returned. Otherwise the endpoint reads the entry assembly's PE resources to construct the response, stores it in memory for one hour, and sends it. If PE resources cannot be obtained, an error is logged and an HTTP 500 response is sent.
    /// </remarks>
    public override async Task HandleAsync(CancellationToken ct)
    {
        if (memoryCache.TryGetValue(nameof(InfoEndpoint), out RootResponse? response) && response is not null)
        {
            await Send.OkAsync(response, ct);
            return;
        }

        string? serverVersion = ApplicationVersionHelper.TryGetEntryAssemblyFileVersion();

        if (serverVersion is null)
        {
            logger.LogError("Couldn't get PE file resources");
            await Send.ErrorsAsync(500, ct);
            return;
        }

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