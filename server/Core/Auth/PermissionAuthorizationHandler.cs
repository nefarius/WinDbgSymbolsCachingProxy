using Microsoft.AspNetCore.Authorization;

using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Core.Auth;

/// <summary>
///     Evaluates <see cref="PermissionRequirement"/> against the current principal.
/// </summary>
/// <remarks>
///     <b>Basic-auth mode</b> (OIDC not enabled):
///     <list type="bullet">
///         <item><c>symbols.download</c> — always succeeds (download stays public, matching pre-OIDC behavior).</item>
///         <item>All other permissions — succeed when the user is authenticated (any Basic-auth user has full access).</item>
///     </list>
///     <b>OIDC mode</b>: the user must carry a <c>permission</c> claim whose value matches the requirement.
///     Claim values are injected by <see cref="AuthSetupExtensions"/> during token validation and by
///     <see cref="ApiKeyAuthenticationHandler"/> during API-key validation.
/// </remarks>
internal sealed class PermissionAuthorizationHandler(OidcConfigProvider oidcConfig)
    : AuthorizationHandler<PermissionRequirement>
{
    private static readonly HashSet<string> AnonymousInBasicMode = [Permissions.SymbolsDownload];

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!oidcConfig.IsOidcEnabled)
        {
            if (AnonymousInBasicMode.Contains(requirement.Permission)
                || context.User.Identity?.IsAuthenticated == true)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        if (context.User.HasClaim(c =>
                c.Type == AuthConstants.PermissionClaimType && c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
