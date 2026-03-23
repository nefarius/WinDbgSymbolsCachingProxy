using Microsoft.AspNetCore.Http;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Copies the incoming request's Authorization header onto outgoing <see cref="HttpClient" /> calls (e.g. Blazor → same-host API).
/// </summary>
internal sealed class ForwardAuthorizationHttpMessageHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of ForwardAuthorizationHttpMessageHandler that forwards the current incoming request's Authorization header onto outgoing HttpClient requests.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current HttpContext used to read the incoming request's Authorization header.</param>
    public ForwardAuthorizationHttpMessageHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Copies the current inbound HTTP context's Authorization header (if present) onto the outgoing <paramref name="request"/> before forwarding it to the next handler.
    /// </summary>
    /// <param name="request">The outgoing HTTP request which may be modified to include the inbound Authorization header.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the operation.</param>
    /// <returns>A task whose result is the <see cref="HttpResponseMessage"/> produced by the next handler in the pipeline.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? auth = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(auth))
        {
            request.Headers.Remove("Authorization");
            request.Headers.TryAddWithoutValidation("Authorization", auth);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
