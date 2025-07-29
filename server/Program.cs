using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;

using Coravel;

using FastEndpoints;
using FastEndpoints.Swagger;

using idunno.Authentication.Basic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.SymbolStore;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Entities;

using MudBlazor.Services;

using Nefarius.Utilities.AspNetCore;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

#region OTEL

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .ConfigureResource(_ => ResourceBuilder.CreateDefault().AddService(TracingSources.AppActivitySourceName))
            .AddSource(TracingSources.AppActivitySourceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    });

#endregion

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

#endregion

#region Misc. Services

builder.Services.AddMemoryCache();
builder.Services.AddScheduler();

#endregion

#region MudBlazor

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
                    new(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String,
                        context.Options.ClaimsIssuer),
                    new(ClaimTypes.Name, context.Username, ClaimValueTypes.String,
                        context.Options.ClaimsIssuer)
                ];

                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                context.Success();

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

#region Database

Log.Logger.Information("Initializing database connection");

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

await DB.InitAsync(serviceConfig.DatabaseName,
    MongoClientSettings.FromConnectionString(serviceConfig.ConnectionString));

Log.Logger.Information("Running database migrations (if any)");

await DB.MigrateAsync();

Log.Logger.Information("Creating index");

await DB.Index<SymbolsEntity>()
    .Key(a => a.IndexPrefix, KeyType.Text)
    .Key(a => a.FileName, KeyType.Text)
    .CreateAsync();

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
    .AddInteractiveServerRenderMode();

await app.RunAsync();