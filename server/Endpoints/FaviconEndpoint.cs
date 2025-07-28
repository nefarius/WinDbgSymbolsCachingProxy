using FastEndpoints;

using Microsoft.Extensions.Primitives;

using Resourcer;

namespace WinDbgSymbolsCachingProxy.Endpoints;

internal sealed class FaviconEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/favicon.ico");
        AllowAnonymous();
        Options(builder => builder.ExcludeFromDescription());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        SetCacheControl(TimeSpan.FromHours(12).Seconds, DateTimeOffset.Now);

        await Send.StreamAsync(Resource.AsStream("WinDbgSymbolsCachingProxy.favicon.ico"), contentType: "image/x-icon",
            cancellation: ct);
    }

    private void SetCacheControl(int expiresSeconds, DateTimeOffset now)
    {
        // cache control, otherwise GitHub etc. will not request an update for like a day or so
        HttpContext.Response.Headers.CacheControl =
            new StringValues($"max-age={expiresSeconds}, s-maxage={expiresSeconds}");
        HttpContext.Response.Headers.Age = new StringValues("0");
        HttpContext.Response.Headers.Date = new StringValues(now.ToString("R"));
        HttpContext.Response.Headers.LastModified = new StringValues(now.ToString("R"));
        HttpContext.Response.Headers.Expires = new StringValues(now.AddSeconds(expiresSeconds).ToString("R"));
    }
}