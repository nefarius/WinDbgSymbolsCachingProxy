using FastEndpoints;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class BadgeEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/badges/{Name}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(ct);
    }
}