using FastEndpoints;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Endpoint for the legacy index2.txt path. Returns 404 to indicate this server does not provide an index file.
/// </summary>
internal sealed class Index2Endpoint : EndpointWithoutRequest
{
    /// <summary>
    ///     Configures the GET route at "/download/symbols/index2.txt" with anonymous access, excluded from API description.
    /// </summary>
    public override void Configure()
    {
        Get("/download/symbols/index2.txt");
        AllowAnonymous();
        Options(builder => builder.ExcludeFromDescription());
    }

    /// <summary>
    ///     Responds with 404 Not Found since this proxy does not serve index2.txt.
    /// </summary>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>A task that completes when the response has been sent.</returns>
    public override Task HandleAsync(CancellationToken ct)
    {
        return Send.NotFoundAsync(ct);
    }
}