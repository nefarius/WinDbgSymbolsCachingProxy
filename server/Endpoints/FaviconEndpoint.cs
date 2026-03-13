using FastEndpoints;

using Microsoft.Extensions.Primitives;

using Resourcer;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Serves the application favicon from embedded resources.
/// </summary>
internal sealed class FaviconEndpoint : EndpointWithoutRequest
{
    /// <summary>
    ///     Configures the GET route at "/favicon.ico" with anonymous access, excluded from API description.
    /// </summary>
    public override void Configure()
    {
        Get("/favicon.ico");
        AllowAnonymous();
        Options(builder => builder.ExcludeFromDescription());
    }

    /// <summary>
    ///     Streams the favicon with cache headers and image/x-icon content type.
    /// </summary>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>A task that completes when the response has been sent.</returns>
    public override async Task HandleAsync(CancellationToken ct)
    {
        SetCacheControl((int)TimeSpan.FromHours(12).TotalSeconds, DateTimeOffset.Now);

        await Send.StreamAsync(Resource.AsStream("WinDbgSymbolsCachingProxy.favicon.ico"), contentType: "image/x-icon",
            cancellation: ct);
    }

    /// <summary>
    ///     Sets HTTP cache-control, age, date, last-modified and expires headers on the response.
    /// </summary>
    /// <param name="expiresSeconds">Max-age and s-maxage in seconds.</param>
    /// <param name="now">Current time used for date-related headers.</param>
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