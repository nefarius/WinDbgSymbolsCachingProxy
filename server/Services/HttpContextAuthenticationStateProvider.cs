using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Supplies Blazor with the current HTTP user so <c>[Authorize]</c> matches Basic auth.
/// </summary>
internal sealed class HttpContextAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsPrincipal user = httpContextAccessor.HttpContext?.User
            ?? new ClaimsPrincipal(new ClaimsIdentity());

        return Task.FromResult(new AuthenticationState(user));
    }
}
