using FastEndpoints;

namespace WinDbgSymbolsCachingProxy.Endpoints;

internal sealed class Index2Endpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/download/symbols/index2.txt");
        AllowAnonymous();
        Options(builder => builder.ExcludeFromDescription());
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return Send.NotFoundAsync(ct);
    }
}