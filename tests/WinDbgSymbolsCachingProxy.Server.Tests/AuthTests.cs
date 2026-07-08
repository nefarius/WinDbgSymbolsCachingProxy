using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Core.Auth;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Server.Tests;

public sealed class ApiKeyServiceHashTests
{
    [Fact]
    public void HashKey_returns_64_char_hex_string()
    {
        string hash = ApiKeyService.HashKey("test-key");
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9a-f]+$", hash);
    }

    [Fact]
    public void HashKey_is_deterministic()
    {
        string hash1 = ApiKeyService.HashKey("same-key");
        string hash2 = ApiKeyService.HashKey("same-key");
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashKey_differs_for_different_inputs()
    {
        Assert.NotEqual(ApiKeyService.HashKey("key-a"), ApiKeyService.HashKey("key-b"));
    }

    [Fact]
    public void GenerateKey_returns_url_safe_string()
    {
        string key = ApiKeyService.GenerateKey();
        Assert.DoesNotContain("+", key);
        Assert.DoesNotContain("/", key);
        Assert.Equal(43, key.Length); // 32 random bytes → 43 base64url chars (no padding)
    }

    [Fact]
    public void GenerateKey_produces_unique_values()
    {
        string key1 = ApiKeyService.GenerateKey();
        string key2 = ApiKeyService.GenerateKey();
        Assert.NotEqual(key1, key2);
    }
}

public sealed class OidcGatedRequirementHandlerTests
{
    private static AuthorizationHandlerContext MakeContext(bool authenticated, OidcGatedRequirement req)
    {
        ClaimsPrincipal user = authenticated
            ? new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "user")], "test"))
            : new ClaimsPrincipal(new ClaimsIdentity());

        return new AuthorizationHandlerContext([req], user, null);
    }

    [Fact]
    public async Task Succeeds_when_oidc_disabled_regardless_of_auth_state()
    {
        OidcConfigProvider provider = new(null);
        OidcGatedRequirementHandler handler = new(provider);
        OidcGatedRequirement req = new();

        AuthorizationHandlerContext ctx = MakeContext(authenticated: false, req);
        await handler.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
    }

    [Fact]
    public async Task Succeeds_when_oidc_enabled_and_user_authenticated()
    {
        Models.OidcConfigEntity config = new() { Enabled = true, Authority = "https://example.com" };
        OidcConfigProvider provider = new(config);
        OidcGatedRequirementHandler handler = new(provider);
        OidcGatedRequirement req = new();

        AuthorizationHandlerContext ctx = MakeContext(authenticated: true, req);
        await handler.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
    }

    [Fact]
    public async Task Fails_when_oidc_enabled_and_user_unauthenticated()
    {
        Models.OidcConfigEntity config = new() { Enabled = true, Authority = "https://example.com" };
        OidcConfigProvider provider = new(config);
        OidcGatedRequirementHandler handler = new(provider);
        OidcGatedRequirement req = new();

        AuthorizationHandlerContext ctx = MakeContext(authenticated: false, req);
        await handler.HandleAsync(ctx);

        Assert.False(ctx.HasSucceeded);
    }
}

public sealed class PermissionAuthorizationHandlerTests
{
    private static ClaimsPrincipal AuthenticatedUser(params string[] permissionClaims)
    {
        List<Claim> claims = [new(ClaimTypes.Name, "user"), .. permissionClaims.Select(p => new Claim(AuthConstants.PermissionClaimType, p))];
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ClaimsPrincipal AnonymousUser()
        => new(new ClaimsIdentity());

    private static AuthorizationHandlerContext MakeContext(ClaimsPrincipal user, PermissionRequirement req)
        => new([req], user, null);

    // ── Basic-auth mode (OIDC off) ──────────────────────────────────────────

    [Fact]
    public async Task BasicMode_SymbolsDownload_succeeds_for_anonymous()
    {
        OidcConfigProvider provider = new(null);
        PermissionAuthorizationHandler handler = new(provider);
        PermissionRequirement req = new(Permissions.SymbolsDownload);

        AuthorizationHandlerContext ctx = MakeContext(AnonymousUser(), req);
        await handler.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
    }

    [Fact]
    public async Task BasicMode_SymbolsUpload_succeeds_for_authenticated()
    {
        OidcConfigProvider provider = new(null);
        PermissionAuthorizationHandler handler = new(provider);
        PermissionRequirement req = new(Permissions.SymbolsUpload);

        AuthorizationHandlerContext ctx = MakeContext(AuthenticatedUser(), req);
        await handler.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
    }

    [Fact]
    public async Task BasicMode_SymbolsUpload_fails_for_anonymous()
    {
        OidcConfigProvider provider = new(null);
        PermissionAuthorizationHandler handler = new(provider);
        PermissionRequirement req = new(Permissions.SymbolsUpload);

        AuthorizationHandlerContext ctx = MakeContext(AnonymousUser(), req);
        await handler.HandleAsync(ctx);

        Assert.False(ctx.HasSucceeded);
    }

    // ── OIDC mode ───────────────────────────────────────────────────────────

    [Fact]
    public async Task OidcMode_succeeds_when_permission_claim_present()
    {
        Models.OidcConfigEntity config = new() { Enabled = true };
        OidcConfigProvider provider = new(config);
        PermissionAuthorizationHandler handler = new(provider);
        PermissionRequirement req = new(Permissions.SymbolsUpload);

        AuthorizationHandlerContext ctx = MakeContext(
            AuthenticatedUser(Permissions.SymbolsUpload), req);
        await handler.HandleAsync(ctx);

        Assert.True(ctx.HasSucceeded);
    }

    [Fact]
    public async Task OidcMode_fails_when_permission_claim_absent()
    {
        Models.OidcConfigEntity config = new() { Enabled = true };
        OidcConfigProvider provider = new(config);
        PermissionAuthorizationHandler handler = new(provider);
        PermissionRequirement req = new(Permissions.Admin);

        // User has upload permission but not admin
        AuthorizationHandlerContext ctx = MakeContext(
            AuthenticatedUser(Permissions.SymbolsUpload), req);
        await handler.HandleAsync(ctx);

        Assert.False(ctx.HasSucceeded);
    }

    [Fact]
    public async Task OidcMode_download_requires_claim_unlike_basic_mode()
    {
        Models.OidcConfigEntity config = new() { Enabled = true };
        OidcConfigProvider provider = new(config);
        PermissionAuthorizationHandler handler = new(provider);
        PermissionRequirement req = new(Permissions.SymbolsDownload);

        // Authenticated but no download permission claim
        AuthorizationHandlerContext ctx = MakeContext(AuthenticatedUser(), req);
        await handler.HandleAsync(ctx);

        Assert.False(ctx.HasSucceeded);
    }
}
