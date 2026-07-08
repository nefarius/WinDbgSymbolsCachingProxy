using Microsoft.AspNetCore.Http;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Copies the incoming request's <c>Authorization</c> header (Basic auth / API key) and
///     <c>Cookie</c> header (OIDC auth cookie) onto outgoing <see cref="HttpClient" /> calls,
///     so that server-side Blazor pages can call same-host REST endpoints as the current user.
/// </summary>
internal sealed class ForwardAuthorizationHttpMessageHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ForwardAuthorizationHttpMessageHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpRequest? inbound = _httpContextAccessor.HttpContext?.Request;
        if (inbound is null)
            return base.SendAsync(request, cancellationToken);

        // Forward the Authorization header (used by Basic auth and API-key clients).
        string? auth = inbound.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(auth))
        {
            request.Headers.Remove("Authorization");
            request.Headers.TryAddWithoutValidation("Authorization", auth);
        }

        // Forward the Cookie header (used by OIDC cookie auth in Blazor Server loops).
        string? cookie = inbound.Headers.Cookie.FirstOrDefault();
        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.Remove("Cookie");
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
