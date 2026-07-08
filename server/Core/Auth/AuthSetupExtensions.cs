using System.Security.Claims;

using idunno.Authentication.Basic;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
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

                    UserEntity user = await userService.GetOrCreateAsync(
                        subject, email, displayName, ctx.HttpContext.RequestAborted);

                    // Bootstrap: grant Admin role if the subject or email is in the seed list.
                    List<string> bootstrapIds = configProvider.Config?.BootstrapAdminIdentifiers ?? [];
                    bool shouldBootstrap = bootstrapIds.Any(id =>
                        string.Equals(id, subject, StringComparison.OrdinalIgnoreCase)
                        || (email is not null && string.Equals(id, email, StringComparison.OrdinalIgnoreCase)));

                    if (shouldBootstrap && !user.RoleNames.Contains("Admin"))
                    {
                        user.RoleNames.Add("Admin");
                        await userService.SaveAsync(user, ctx.HttpContext.RequestAborted);
                    }

                    List<string> permissions = await roleService.GetPermissionsForRolesAsync(
                        user.RoleNames, ctx.HttpContext.RequestAborted);

                    ClaimsIdentity appIdentity = new();
                    appIdentity.AddClaims(user.RoleNames.Select(r => new Claim(ClaimTypes.Role, r)));
                    appIdentity.AddClaims(permissions.Select(p =>
                        new Claim(AuthConstants.PermissionClaimType, p)));

                    ctx.Principal.AddIdentity(appIdentity);
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName, _ => { });
    }

    private static bool IsApiRequest(HttpRequest request)
        => request.Path.StartsWithSegments("/api")
            || (request.Headers.TryGetValue("Accept", out Microsoft.Extensions.Primitives.StringValues accept)
                && accept.Any(a => a?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true));
}
