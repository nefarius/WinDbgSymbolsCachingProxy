using FastEndpoints;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Protected endpoint used to trigger a browser Basic-Auth challenge.
/// </summary>
public sealed class AuthChallengeEndpoint : EndpointWithoutRequest
{
    /// <summary>
    ///     Configures a protected auth check endpoint at a root-level path.
    /// </summary>
    public override void Configure()
    {
        Get("/auth-challenge");
        Policy(p => p.RequireAuthenticatedUser());
    }

    /// <summary>
    ///     Returns 200 when authenticated.
    /// </summary>
    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync("Authenticated.", ct);
    }
}
