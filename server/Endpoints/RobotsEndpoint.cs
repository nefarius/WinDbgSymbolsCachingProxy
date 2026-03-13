using FastEndpoints;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Serves robots.txt instructing crawlers to disallow all paths.
/// </summary>
internal sealed class RobotsEndpoint : EndpointWithoutRequest
{
    /// <summary>
    ///     Configures the GET route at "/robots.txt" with anonymous access, excluded from API description.
    /// </summary>
    public override void Configure()
    {
        Get("/robots.txt");
        AllowAnonymous();
        Options(builder => builder.ExcludeFromDescription());
    }

    /// <summary>
    ///     Returns a robots.txt body with "User-agent: *" and "Disallow: /".
    /// </summary>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>A task that completes when the response has been sent.</returns>
    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.StringAsync("""
                               User-agent: *
                               Disallow: /
                               """, cancellation: ct);
    }
}