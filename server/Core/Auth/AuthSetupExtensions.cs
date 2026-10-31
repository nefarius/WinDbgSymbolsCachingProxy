using System.Security.Claims;

using idunno.Authentication.Basic;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Core.Auth;

/// <summary>
///     Configures authentication and authorization depending on whether OIDC is enabled.
/// </summary>
internal static class AuthSetupExtensions
{
    /// <summary>
    ///     Registers authentication schemes and authorization policies.
    ///     Call after <see cref="OidcConfigProvider"/> is registered in DI.
    /// </summary>
    public static IServiceCollection AddAuthSetup(
        this IServiceCollection services,
        OidcConfigEntity? oidcConfig)
    {
        bool oidcEnabled = oidcConfig is { Enabled: true };

        if (oidcEnabled)
        {
            RegisterOidcSchemes(services, oidcConfig!);
        }
        else
        {
            RegisterBasicAuthScheme(services);
        }

        services.AddSingleton<IAuthorizationHandler, OidcGatedRequirementHandler>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            // Endpoints / pages with no explicit auth metadata use this policy.
            // It is transparent in Basic-auth mode and enforces login in OIDC mode.
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(new OidcGatedRequirement())
                .Build();

            foreach (string permission in Permissions.All)
            {
                string p = permission;
                options.AddPolicy(p, policy => policy.AddRequirements(new PermissionRequirement(p)));
            }
        });

        return services;
    }

    // ── Basic auth ──────────────────────────────────────────────────────────

    private static void RegisterBasicAuthScheme(IServiceCollection services)
    {
        services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
            .AddBasic(options =>
            {
                options.Realm = "Basic Authentication";
                // idunno returns 421 (not 401) on HTTP unless this is true.
                // Use only on trusted networks; prefer HTTPS when exposed beyond the LAN.
                options.AllowInsecureProtocol = true;
                options.Events = new BasicAuthenticationEvents
                {
                    OnValidateCredentials = context =>
                    {
                        ServiceConfig config = context.HttpContext.RequestServices
                            .GetRequiredService<IOptions<ServiceConfig>>()
                            .Value;

                        BasicAuthCredentials credential = new()
                        {
                            Username = context.Username,
                            Password = context.Password
                        };

                        if (config.BasicAuthCredentials == null ||
                            !config.BasicAuthCredentials.Contains(credential))
                        {
                            LogRejectedCredentials(context);
                            return Task.CompletedTask;
                        }

                        // Grant every permission to any successfully authenticated user in Basic mode.
                        List<Claim> claims =
                        [
                            new(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String,
                                context.Options.ClaimsIssuer),
                            new(ClaimTypes.Name, context.Username, ClaimValueTypes.String,
                                context.Options.ClaimsIssuer),
                            .. Permissions.All.Select(p => new Claim(
                                AuthConstants.PermissionClaimType, p, ClaimValueTypes.String,
                                context.Options.ClaimsIssuer))
                        ];

                        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                        context.Success();
                        return Task.CompletedTask;
                    }
                };
            });
    }

    /// <summary>
    ///     Logs that supplied Basic credentials were rejected, without recording username or password.
    /// </summary>
    private static void LogRejectedCredentials(ValidateCredentialsContext context)
    {
        ILogger logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("WinDbgSymbolsCachingProxy.Core.Auth.BasicAuth");

        BasicAuthChallengeInfo info = BasicAuthChallengeDiagnostics.Capture(
            context.HttpContext,
            usernamePresent: !string.IsNullOrEmpty(context.Username));

        logger.LogWarning(
            BasicAuthChallengeDiagnostics.RejectionMessageTemplate,
            info.Method,
            info.Path,
            info.UsernamePresent,
            info.AuthorizationHeaderPresent,
            info.RemoteIp,
            info.UserAgent,
            info.TraceId);
    }

    // ── OIDC + Cookie + ApiKey ───────────────────────────────────────────────

    private static void RegisterOidcSchemes(IServiceCollection services, OidcConfigEntity oidcConfig)
    {
        services
            .AddAuthentication(options =>
            {
                // "Auto" policy scheme inspects each request and forwards to the right handler.
                options.DefaultScheme = AuthConstants.AutoSchemeName;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddPolicyScheme(AuthConstants.AutoSchemeName, "Auto-select Cookie or ApiKey", options =>
            {
                options.ForwardDefaultSelector = ctx =>
                {
                    // Requests carrying an API key header or query param go to the ApiKey handler.
                    if (ctx.Request.Headers.ContainsKey(ApiKeyAuthenticationHandler.HeaderName)
                        || ctx.Request.Query.ContainsKey(ApiKeyAuthenticationHandler.QueryParamName))
                    {
                        return ApiKeyAuthenticationHandler.SchemeName;
                    }

                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/access-denied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);

                string cookieSubjectClaim = oidcConfig.SubjectClaimType;
                options.Events.OnValidatePrincipal = ctx => ValidatePrincipalAsync(ctx, cookieSubjectClaim);

                // API requests should receive 401, not a login redirect.
                options.Events.OnRedirectToLogin = ctx =>
                {
                    if (IsApiRequest(ctx.Request))
                    {
                        ctx.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }

                    ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    if (IsApiRequest(ctx.Request))
                    {
                        ctx.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    }

                    ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = oidcConfig.Authority;
                options.ClientId = oidcConfig.ClientId;
                options.ClientSecret = oidcConfig.ClientSecret;
                options.ResponseType = "code";
                options.SaveTokens = false;
                options.GetClaimsFromUserInfoEndpoint = true;

                foreach (string scope in oidcConfig.Scopes)
                {
                    if (!options.Scope.Contains(scope))
                        options.Scope.Add(scope);
                }

                options.TokenValidationParameters.NameClaimType = oidcConfig.NameClaimType;

                string subjectClaimType = oidcConfig.SubjectClaimType;
                string emailClaimType = oidcConfig.EmailClaimType;
                string nameClaimType = oidcConfig.NameClaimType;

                options.Events.OnTokenValidated = async ctx =>
                {
                    if (ctx.Principal is null)
                        return;

                    UserService userService = ctx.HttpContext.RequestServices.GetRequiredService<UserService>();
                    RoleService roleService = ctx.HttpContext.RequestServices.GetRequiredService<RoleService>();
                    OidcConfigProvider configProvider =
                        ctx.HttpContext.RequestServices.GetRequiredService<OidcConfigProvider>();

                    string? subject = ctx.Principal.FindFirstValue(subjectClaimType)
                        ?? ctx.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                    string? email = ctx.Principal.FindFirstValue(emailClaimType)
                        ?? ctx.Principal.FindFirstValue(ClaimTypes.Email);
                    string? displayName = ctx.Principal.FindFirstValue(nameClaimType)
                        ?? ctx.Principal.FindFirstValue(ClaimTypes.Name);

                    if (subject is null)
                        return;

                    // Never trust role/permission claims supplied by the identity provider; only the
                    // application's own identity (built below from the database) may grant access.
                    StripAuthorizationClaims(ctx.Principal);

                    (UserEntity user, bool created) = await userService.GetOrCreateWithStatusAsync(
                        subject, email, displayName, ctx.HttpContext.RequestAborted);

                    // Bootstrap: grant Admin role if the subject or a verified e-mail is in the seed list,
                    // but only for a brand-new user or while no administrator exists (so admins can demote
                    // bootstrap users and a compromised identifier cannot silently regain access).
                    List<string> bootstrapIds = configProvider.Config?.BootstrapAdminIdentifiers ?? [];
                    bool emailVerified = string.Equals(
                        ctx.Principal.FindFirstValue("email_verified"), "true", StringComparison.OrdinalIgnoreCase);
                    bool shouldBootstrap = bootstrapIds.Any(id =>
                        string.Equals(id, subject, StringComparison.OrdinalIgnoreCase)
                        || (emailVerified && email is not null
                            && string.Equals(id, email, StringComparison.OrdinalIgnoreCase)));

                    if (shouldBootstrap && !user.RoleNames.Contains("Admin")
                        && (created || !await userService.AnyAdminAsync(ctx.HttpContext.RequestAborted)))
                    {
                        await userService.AddRoleAsync(user, "Admin", ctx.HttpContext.RequestAborted);
                    }

                    ctx.Principal.AddIdentity(await BuildAppIdentityAsync(
                        user, roleService, ctx.HttpContext.RequestAborted));
                };

                options.Events.OnRedirectToIdentityProvider = ctx =>
                {
                    // Non-interactive clients (WinDbg, CI, XHR) must receive 401, not a login redirect.
                    if (IsApiRequest(ctx.Request) || !AcceptsHtml(ctx.Request))
                    {
                        ctx.Response.StatusCode = 401;
                        ctx.HandleResponse();
                    }

                    return Task.CompletedTask;
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName, _ => { });
    }

    /// <summary>Authentication type of the identity that carries database-derived roles and permissions.</summary>
    internal const string AppIdentityAuthenticationType = "WinDbgSymbolsCachingProxy.App";

    private static readonly TimeSpan PrincipalRevalidationInterval = TimeSpan.FromSeconds(30);

    private sealed record CachedUser(UserEntity? User);

    private static void StripAuthorizationClaims(ClaimsPrincipal principal)
    {
        foreach (ClaimsIdentity identity in principal.Identities)
        {
            foreach (Claim claim in identity.Claims
                         .Where(c => c.Type == AuthConstants.PermissionClaimType || c.Type == ClaimTypes.Role)
                         .ToList())
            {
                identity.RemoveClaim(claim);
            }
        }
    }

    private static async Task<ClaimsIdentity> BuildAppIdentityAsync(
        UserEntity user, RoleService roleService, CancellationToken ct)
    {
        List<string> permissions = await roleService.GetPermissionsForRolesAsync(user.RoleNames, ct);

        ClaimsIdentity appIdentity = new(AppIdentityAuthenticationType);
        appIdentity.AddClaims(user.RoleNames.Select(r => new Claim(ClaimTypes.Role, r)));
        appIdentity.AddClaims(permissions.Select(p => new Claim(AuthConstants.PermissionClaimType, p)));
        return appIdentity;
    }

    /// <summary>
    ///     Re-reads the user (briefly cached) on cookie validation so that removed roles or deleted users take
    ///     effect without waiting for the cookie to expire.
    /// </summary>
    private static async Task ValidatePrincipalAsync(
        CookieValidatePrincipalContext ctx, string subjectClaimType)
    {
        if (ctx.Principal is null)
            return;

        string? subject = ctx.Principal.FindFirstValue(subjectClaimType)
                          ?? ctx.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (subject is null)
            return;

        IServiceProvider sp = ctx.HttpContext.RequestServices;
        Microsoft.Extensions.Caching.Memory.IMemoryCache cache =
            sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
        string cacheKey = $"auth-user:{subject}";

        if (!cache.TryGetValue(cacheKey, out CachedUser? cached) || cached is null)
        {
            UserEntity? found = await sp.GetRequiredService<UserService>()
                .FindBySubjectAsync(subject, ctx.HttpContext.RequestAborted);
            cached = new CachedUser(found);
            cache.Set(cacheKey, cached, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PrincipalRevalidationInterval,
                Size = 1
            });
        }

        if (cached.User is null)
        {
            ctx.RejectPrincipal();
            await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        ClaimsIdentity fresh = await BuildAppIdentityAsync(
            cached.User, sp.GetRequiredService<RoleService>(), ctx.HttpContext.RequestAborted);

        ClaimsPrincipal replacement = new(ctx.Principal.Identities
            .Where(i => i.AuthenticationType != AppIdentityAuthenticationType));
        replacement.AddIdentity(fresh);
        ctx.ReplacePrincipal(replacement);
    }

    private static bool AcceptsHtml(HttpRequest request)
        => request.Headers.TryGetValue("Accept", out Microsoft.Extensions.Primitives.StringValues accept)
           && accept.Any(a => a?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true);

    private static bool IsApiRequest(HttpRequest request)
        => request.Path.StartsWithSegments("/api")
            || request.Path.StartsWithSegments("/download")
            || (request.Headers.TryGetValue("Accept", out Microsoft.Extensions.Primitives.StringValues accept)
                && accept.Any(a => a?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true));
}
