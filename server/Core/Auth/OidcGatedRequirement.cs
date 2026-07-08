using Microsoft.AspNetCore.Authorization;

using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Core.Auth;

/// <summary>
///     Authorization requirement that succeeds unconditionally when OIDC is not enabled,
///     and requires an authenticated user when OIDC is active.
///     Used as the <see cref="AuthorizationOptions.FallbackPolicy"/> so that routes with no
///     explicit policy are open when running in Basic-auth mode and secured when in OIDC mode.
/// </summary>
internal sealed class OidcGatedRequirement : IAuthorizationRequirement { }

internal sealed class OidcGatedRequirementHandler(OidcConfigProvider oidcConfig)
    : AuthorizationHandler<OidcGatedRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OidcGatedRequirement requirement)
    {
        if (!oidcConfig.IsOidcEnabled || context.User.Identity?.IsAuthenticated == true)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
