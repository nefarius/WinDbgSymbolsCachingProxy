using FastEndpoints;

using Microsoft.Extensions.Primitives;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Serves a PNG Open Graph image for the public status page (live counts, cached with /info).
/// </summary>
public sealed class StatusOpenGraphImageEndpoint(
    ICachedSymbolOverviewProvider overviewProvider,
    IStatusOpenGraphImageRenderer renderer)
    : EndpointWithoutRequest
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/og/status.png");
        AllowAnonymous();
        Options(builder => builder.ExcludeFromDescription());
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        RootResponse overview = await overviewProvider.GetAsync(ct);

        using MemoryStream ms = new();
        renderer.Render(overview, ms);
        ms.Position = 0;

        DateTime now = DateTime.UtcNow;
        const int expiresSeconds = 3600;
        HttpContext.Response.Headers.CacheControl =
            new StringValues($"public, max-age={expiresSeconds}, s-maxage={expiresSeconds}");
        HttpContext.Response.Headers.Age = new StringValues("0");
        HttpContext.Response.Headers.Date = new StringValues(now.ToString("R"));
        HttpContext.Response.Headers.LastModified = new StringValues(now.ToString("R"));
        HttpContext.Response.Headers.Expires = new StringValues(now.AddSeconds(expiresSeconds).ToString("R"));

        await Send.StreamAsync(ms, contentType: "image/png", cancellation: ct);
    }
}
