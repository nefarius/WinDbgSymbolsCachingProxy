using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     OIDC login/logout minimal-API endpoints.
/// </summary>
internal static class AccountEndpoints
{
    /// <summary>
    ///     Registered even in Basic-auth mode, where they just redirect to the home page harmlessly.
    /// </summary>
    public static WebApplication MapAccountEndpoints(this WebApplication app, OidcConfigProvider oidcConfigProvider)
    {
        app.MapGet("/account/login", (HttpContext ctx, string? returnUrl) =>
        {
            if (!oidcConfigProvider.IsOidcEnabled)
            {
                return Results.Redirect("/");
            }

            // Reject external URLs to prevent open-redirect attacks.
            string safeReturn = IsLocalUrl(returnUrl) ? returnUrl! : "/";

            if (ctx.User.Identity?.IsAuthenticated == true)
            {
                return Results.Redirect(safeReturn);
            }

            AuthenticationProperties props = new() { RedirectUri = safeReturn };
            return Results.Challenge(props, [OpenIdConnectDefaults.AuthenticationScheme]);
        }).AllowAnonymous();

        app.MapGet("/account/logout", async (HttpContext ctx) =>
        {
            if (!oidcConfigProvider.IsOidcEnabled)
            {
                ctx.Response.Redirect("/");
                return;
            }

            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = "/" });
        }).AllowAnonymous();

        return app;
    }

    /// <summary>
    ///     Returns true only for relative same-site paths, preventing open redirects via a crafted returnUrl.
    /// </summary>
    internal static bool IsLocalUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // Browsers strip tabs/newlines, so "/\t/evil.com" would become "//evil.com"; reject control characters.
        if (url.Any(char.IsControl))
        {
            return false;
        }

        // Relative path (most common case: "/search", "/upload"); "//host" and "/\host" are protocol-relative.
        return url.StartsWith('/') && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));
    }
}
