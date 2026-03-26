using FastEndpoints;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Protected endpoint used to trigger a browser Basic-Auth challenge and optionally return to a page afterward.
/// </summary>
public sealed class AuthChallengeEndpoint : EndpointWithoutRequest
{
    /// <summary>
    ///     Configures a protected auth check endpoint at "/api/auth/challenge".
    /// </summary>
    public override void Configure()
    {
        Get("/api/auth/challenge");
        Policy(p => p.RequireAuthenticatedUser());
    }

    /// <summary>
    ///     Returns 200 when authenticated, or redirects to a local return URL when provided.
    /// </summary>
    public override async Task HandleAsync(CancellationToken ct)
    {
        string? returnUrl = Query<string?>("returnUrl", false);

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Uri.TryCreate(returnUrl, UriKind.Relative, out _))
        {
            HttpContext.Response.Redirect(returnUrl);
            return;
        }

        await Send.OkAsync("Authenticated.", ct);
    }
}
