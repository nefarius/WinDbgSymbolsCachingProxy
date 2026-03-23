using FastEndpoints;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class InfoEndpoint(ICachedSymbolOverviewProvider overviewProvider)
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
    /// Delegates to <see cref="ICachedSymbolOverviewProvider" />, which applies the same one-hour in-memory cache used for Open Graph image generation.
    /// </remarks>
    public override async Task HandleAsync(CancellationToken ct)
    {
        RootResponse response = await overviewProvider.GetAsync(ct);
        await Send.OkAsync(response, ct);
    }
}