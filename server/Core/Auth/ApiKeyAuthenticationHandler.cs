using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Core.Auth;

/// <summary>
///     Authentication scheme for non-interactive clients. Reads the API key from the
///     <c>X-Api-Key</c> request header or the <c>apiKey</c> query-string parameter,
///     validates it against <see cref="ApiKeyEntity"/> records, and emits role and permission claims.
/// </summary>
internal sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ApiKeyService apiKeyService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string SchemeName = "ApiKey";
    internal const string HeaderName = "X-Api-Key";
    internal const string QueryParamName = "apiKey";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? key = Request.Headers[HeaderName].FirstOrDefault()
            ?? Request.Query[QueryParamName].FirstOrDefault();

        if (string.IsNullOrEmpty(key))
            return AuthenticateResult.NoResult();

        ApiKeyEntity? apiKey = await apiKeyService.ValidateAsync(key, Context.RequestAborted);
        if (apiKey is null)
            return AuthenticateResult.Fail("Invalid or expired API key.");

        List<string> permissions = await apiKeyService.GetEffectivePermissionsAsync(apiKey, Context.RequestAborted);

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, apiKey.ID, ClaimValueTypes.String, Scheme.Name),
            new(ClaimTypes.Name, apiKey.Name, ClaimValueTypes.String, Scheme.Name),
            .. apiKey.RoleNames.Select(r => new Claim(ClaimTypes.Role, r, ClaimValueTypes.String, Scheme.Name)),
            .. permissions.Select(p => new Claim(AuthConstants.PermissionClaimType, p, ClaimValueTypes.String, Scheme.Name))
        ];

        ClaimsIdentity identity = new(claims, Scheme.Name);
        ClaimsPrincipal principal = new(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }

    /// <summary>Returns HTTP 401 without a redirect — API clients must not be sent to an OIDC login page.</summary>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        return Task.CompletedTask;
    }
}
