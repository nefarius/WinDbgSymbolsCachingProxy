using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;

using Coravel;

using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.SymbolStore;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Entities;

using MudBlazor.Services;

using Nefarius.Utilities.AspNetCore;

using Polly;
using Polly.Contrib.WaitAndRetry;

using Serilog;

using WinDbgSymbolsCachingProxy.Components;
using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Core.Auth;
using WinDbgSymbolsCachingProxy.Jobs;
using WinDbgSymbolsCachingProxy.Logging;
using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

using Tracer = WinDbgSymbolsCachingProxy.Core.Tracer;

WebApplicationOptions opts = new() { Args = args, ContentRootPath = AppContext.BaseDirectory };

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

LogBufferService logBuffer = new();

WebApplicationBuilder? builder = WebApplication.CreateBuilder(opts).Setup(options =>
{
    options.Serilog.Configuration.WriteTo.Sink(new LogBufferSink(logBuffer));
});

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    Log.Logger.Information("Configuring Windows Service");

    builder.Host.UseWindowsService(options =>
    {
        options.ServiceName = TracingSources.AppActivitySourceName;
    });
}

#region Configuration

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 200_000_000; // 200 MB
});

builder.Services.AddHostedService<StartupService>();

IConfigurationSection section = builder.Configuration.GetSection(nameof(ServiceConfig));

ServiceConfig? serviceConfig = section.Get<ServiceConfig>();

if (serviceConfig is null)
{
    Console.WriteLine("Missing service configuration, can't continue!");
    return;
}

builder.Services.Configure<ServiceConfig>(builder.Configuration.GetSection(nameof(ServiceConfig)));

#endregion

#region Database

// Database must be initialized before auth setup so that OidcConfigEntity can be loaded.
Log.Logger.Information("Initializing database connection");

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

DB db = await DB.InitAsync(serviceConfig.DatabaseName,
    MongoClientSettings.FromConnectionString(serviceConfig.ConnectionString));

builder.Services.AddSingleton(db);

// Load OIDC configuration (single document) from MongoDB.
OidcConfigEntity? oidcConfigDoc = await db.Find<OidcConfigEntity>().ExecuteFirstAsync();
OidcConfigProvider oidcConfigProvider = new(oidcConfigDoc);
builder.Services.AddSingleton(oidcConfigProvider);

#endregion

#region Core Services

builder.Services.AddSingleton<IBadgeFactory, BadgeFactory>();
builder.Services.AddSingleton<IBadgeService, BadgeService>();
builder.Services.AddSingleton<ISvgService, SvgService>();
builder.Services.AddTransient<RecheckNotFoundJob>();
builder.Services.AddSingleton<SymbolAliasLookupService>();
builder.Services.AddSingleton<RecheckNotFoundService>();
builder.Services.AddTransient<SymStoreService>();
builder.Services.AddTransient<ITracer, Tracer>();
builder.Services.AddSingleton<SymbolParsingService>();
builder.Services.AddSingleton<ICachedSymbolOverviewProvider, CachedSymbolOverviewProvider>();
builder.Services.AddSingleton<IStatusOpenGraphImageRenderer, StatusOpenGraphImageRenderer>();
builder.Services.AddSingleton(logBuffer);

// Auth-related services
builder.Services.AddSingleton<RoleService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ApiKeyService>();

#endregion

#region Misc. Services

builder.Services.AddMemoryCache(cacheOptions =>
{
    cacheOptions.SizeLimit = serviceConfig.MemoryCacheSizeLimit;
});
builder.Services.AddScheduler();

#endregion

#region MudBlazor

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, HttpContextAuthenticationStateProvider>();
builder.Services.AddTransient<ForwardAuthorizationHttpMessageHandler>();
builder.Services.AddHttpClient("SymbolUploadApi")
    .AddHttpMessageHandler<ForwardAuthorizationHttpMessageHandler>();

builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

#endregion

#region REST

builder.Services.AddFastEndpoints().SwaggerDocument(o =>
{
    o.RemoveEmptyRequestSchema = true;
    o.DocumentSettings = s =>
    {
        s.Title = "Symbols Caching Server API";
        s.Version = "v1";
        o.AutoTagPathSegmentIndex = 0;
        o.TagDescriptions = t =>
        {
            t["Symbols"] = "Symbols up- and download endpoints";
            t["Badges"] = "Badge creation endpoints";
        };
    };
});

#endregion

builder.Services.AddHttpClient("MicrosoftSymbolServer",
        client =>
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(
                Assembly.GetEntryAssembly()?.GetName().Name!,
                Assembly.GetEntryAssembly()?.GetName().Version!.ToString()));

            client.BaseAddress = new Uri(serviceConfig.UpstreamUrl);
        })
    .AddTransientHttpErrorPolicy(pb =>
        pb.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(3), 10)));

#region Authentication & Authorization

// Persist Data Protection keys so the cookie auth ticket survives service restarts.
// On Windows this defaults to a per-user DPAPI key; force file-system persistence for
// predictable behaviour across accounts (e.g. running as SYSTEM as a Windows Service).
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "dp-keys")));

// Conditionally registers Basic auth (when OIDC is off) or Cookie+OIDC+ApiKey (when on).
// Do not use a global fallback RequireAuthenticatedUser: it also applies to Blazor's interactive
// server endpoints (circuit/negotiate), which cannot all be opted out via MapRazorComponents.
// FastEndpoints are secured via Policy(...) per endpoint; Blazor uses [Authorize] on pages.
builder.Services.AddAuthSetup(oidcConfigDoc);

#endregion

WebApplication? app = builder.Build().Setup();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
}

app.Services.UseScheduler(scheduler =>
{
    scheduler
        .Schedule<RecheckNotFoundJob>()
        .DailyAtHour(3);
});

// https://github.com/dotnet/aspnetcore/issues/23949#issuecomment-950471048
app.UseMiddleware<CustomExceptionHandlerMiddleware>();

app.UseSwaggerGen();

app.UseAuthentication();
app.UseAuthorization();

// OIDC login/logout minimal-API endpoints.
// These are registered even in Basic-auth mode (they just return 404 / redirect-to-home harmlessly).
app.MapGet("/account/login", (HttpContext ctx, string? returnUrl) =>
{
    if (!oidcConfigProvider.IsOidcEnabled)
        return Results.Redirect("/");

    if (ctx.User.Identity?.IsAuthenticated == true)
        return Results.Redirect(returnUrl ?? "/");

    AuthenticationProperties props = new()
    {
        RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl
    };
    return Results.Challenge(props, [Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme]);
}).AllowAnonymous();

app.MapGet("/account/logout", async (HttpContext ctx) =>
{
    if (!oidcConfigProvider.IsOidcEnabled)
    {
        ctx.Response.Redirect("/");
        return;
    }

    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/" });
}).AllowAnonymous();

app.UseFastEndpoints();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

await app.RunAsync();
