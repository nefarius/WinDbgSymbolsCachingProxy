using FastEndpoints;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class Index2Endpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/download/symbols/index2.txt");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return SendNotFoundAsync(ct);
    }
}