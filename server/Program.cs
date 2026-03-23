using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;

using Coravel;

using FastEndpoints;
using FastEndpoints.Swagger;

using idunno.Authentication.Basic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
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
using WinDbgSymbolsCachingProxy.Jobs;
using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

using Tracer = WinDbgSymbolsCachingProxy.Core.Tracer;

WebApplicationOptions opts = new() { Args = args, ContentRootPath = AppContext.BaseDirectory };

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

WebApplicationBuilder? builder = WebApplication.CreateBuilder(opts).Setup();

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

#region Core Services

builder.Services.AddSingleton<IBadgeFactory, BadgeFactory>();
builder.Services.AddSingleton<IBadgeService, BadgeService>();
builder.Services.AddSingleton<ISvgService, SvgService>();
builder.Services.AddTransient<RecheckNotFoundJob>();
builder.Services.AddSingleton<RecheckNotFoundService>();
builder.Services.AddTransient<SymStoreService>();
builder.Services.AddTransient<ITracer, Tracer>();
builder.Services.AddSingleton<SymbolParsingService>();
builder.Services.AddSingleton<ICachedSymbolOverviewProvider, CachedSymbolOverviewProvider>();
builder.Services.AddSingleton<IStatusOpenGraphImageRenderer, StatusOpenGraphImageRenderer>();

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

// adds Basic Auth for protected endpoints
builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddBasic(options =>
    {
        options.Realm = "Basic Authentication";
        options.Events = new BasicAuthenticationEvents
        {
            OnValidateCredentials = context =>
            {
                // valid credentials pulled from appsettings.json
                ServiceConfig config = context.HttpContext.RequestServices
                    .GetRequiredService<IOptions<ServiceConfig>>()
                    .Value;

                BasicAuthCredentials credential = new() { Username = context.Username, Password = context.Password };

                if (config.BasicAuthCredentials == null ||
                    !config.BasicAuthCredentials.Contains(credential))
                {
                    return Task.CompletedTask;
                }

                Claim[] claims =
                [
                    new Claim(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String,
                        context.Options.ClaimsIssuer),
                    new Claim(ClaimTypes.Name, context.Username, ClaimValueTypes.String,
                        context.Options.ClaimsIssuer)
                ];

                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                context.Success();

                return Task.CompletedTask;
            }
        };
    });

// Do not use a global fallback RequireAuthenticatedUser: it also applies to Blazor's interactive
// server endpoints (circuit/negotiate), which cannot all be opted out via MapRazorComponents.
// FastEndpoints are secure by default unless AllowAnonymous() is called; Blazor uses [Authorize] / [AllowAnonymous] on pages.
builder.Services.AddAuthorization();

#region Database

Log.Logger.Information("Initializing database connection");

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

DB db = await DB.InitAsync(serviceConfig.DatabaseName,
    MongoClientSettings.FromConnectionString(serviceConfig.ConnectionString));

builder.Services.AddSingleton(db);

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

app.UseFastEndpoints();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

await app.RunAsync();