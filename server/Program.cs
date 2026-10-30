using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;

using Coravel;

using FastEndpoints;
using FastEndpoints.Swagger;

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

using Serilog;

using WinDbgSymbolsCachingProxy.Components;
using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Endpoints;
using WinDbgSymbolsCachingProxy.UpdateCheck;
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

// An incomplete OIDC configuration would lock everyone out; fall back to Basic auth instead.
if (oidcConfigDoc is { Enabled: true } &&
    (!Uri.TryCreate(oidcConfigDoc.Authority, UriKind.Absolute, out _) ||
     string.IsNullOrWhiteSpace(oidcConfigDoc.ClientId)))
{
    Log.Logger.Error(
        "OIDC is enabled but Authority or ClientId is missing/invalid; falling back to Basic authentication");
    oidcConfigDoc = null;
}

if (oidcConfigDoc is not { Enabled: true } &&
    serviceConfig.BasicAuthCredentials?.Any(c => c is { Username: "admin", Password: "admin" }) == true)
{
    Log.Logger.Warning(
        "Default Basic-auth credentials admin/admin are active; change them in appsettings before exposing this service");
}

OidcConfigProvider oidcConfigProvider = new(oidcConfigDoc);
builder.Services.AddSingleton(oidcConfigProvider);

#endregion

#region Core Services

builder.Services.AddSymbolProxyServices(logBuffer);

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
builder.Services.AddHttpClient("SymbolUploadApi", client =>
    {
        // Large symbols can take longer than HttpClient's default 100-second timeout to transfer and process.
        client.Timeout = TimeSpan.FromMinutes(30);
    })
    .AddHttpMessageHandler<ForwardAuthorizationHttpMessageHandler>();
builder.Services.AddSingleton<ILocalAssemblyVersionSource, ServerAssemblyVersionSource>();
builder.Services.AddLatestMsiVersionCheck();

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
            AssemblyName entryName = typeof(Program).Assembly.GetName();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(
                entryName.Name ?? "WinDbgSymbolsCachingProxy",
                entryName.Version?.ToString()));

            client.BaseAddress = new Uri(serviceConfig.UpstreamUrl);
            // Timeouts are enforced by the resilience pipeline below.
            client.Timeout = Timeout.InfiniteTimeSpan;
        })
    .AddStandardResilienceHandler(options =>
    {
        // Bounded retries plus a circuit breaker avoid retry storms during upstream outages.
        options.Retry.MaxRetryAttempts = 4;
        options.Retry.Delay = TimeSpan.FromSeconds(3);
        options.Retry.UseJitter = true;
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(2);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(8);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(4);
    });

#region Authentication & Authorization

// Persist Data Protection keys so the cookie auth ticket survives service restarts.
// On Windows this defaults to a per-user DPAPI key; force file-system persistence for
// predictable behaviour across accounts (e.g. running as SYSTEM as a Windows Service).
DirectoryInfo dataProtectionKeys = DataProtectionSetup.ResolveKeysDirectory(serviceConfig.DataProtectionKeysPath);
IDataProtectionBuilder dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("WinDbgSymbolsCachingProxy")
    .PersistKeysToFileSystem(dataProtectionKeys);

if (OperatingSystem.IsWindows())
{
    // Encrypt keys at rest so reading the key directory is not enough to forge auth cookies.
    dataProtection.ProtectKeysWithDpapi(protectToLocalMachine: true);
}

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

// Observe completed responses so Basic 401 challenges include path/method context.
app.UseMiddleware<BasicAuthChallengeDiagnosticsMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapAccountEndpoints(oidcConfigProvider);

app.UseFastEndpoints(c =>
{
    // FastEndpoints adds a generated policy that always RequireAuthenticatedUser() when
    // Policies() is set and AllowAnonymous() is not. That would prompt WinDbg for Basic
    // auth on /download/symbols even though PermissionAuthorizationHandler allows
    // anonymous downloads in Basic-auth mode. Keep the generated policy in OIDC mode.
    if (!oidcConfigProvider.IsOidcEnabled)
    {
        c.Endpoints.Configurator = ep =>
        {
            if (ep.EndpointType == typeof(SymbolsDownloadEndpoint))
            {
                ep.AllowAnonymous();
            }
        };
    }
});
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

await app.RunAsync();
