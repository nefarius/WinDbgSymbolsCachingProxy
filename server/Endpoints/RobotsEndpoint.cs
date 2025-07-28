using FastEndpoints;

namespace WinDbgSymbolsCachingProxy.Endpoints;

internal sealed class RobotsEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/robots.txt");
        AllowAnonymous();
        Options(builder => builder.ExcludeFromDescription());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.StringAsync("""
                              User-agent: *
                              Disallow: /
                              """, cancellation: ct);
    }
}