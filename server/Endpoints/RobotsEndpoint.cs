using FastEndpoints;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class RobotsEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/robots.txt");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendStringAsync(@"User-agent: *
Disallow: /", cancellation: ct);
    }
}