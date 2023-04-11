using FastEndpoints;

using Resourcer;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class FaviconEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/favicon.ico");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendStreamAsync(Resource.AsStream("WinDbgSymbolsCachingProxy.favicon.ico"), contentType: "image/x-icon",
            cancellation: ct);
    }
}