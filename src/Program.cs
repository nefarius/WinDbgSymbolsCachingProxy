using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;

using Coravel;

using FastEndpoints;

using idunno.Authentication.Basic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.SymbolStore;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Entities;

using Nefarius.Utilities.AspNetCore;

using Polly;
using Polly.Contrib.WaitAndRetry;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Jobs;
using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args).Setup();

builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 200_000_000; // 200 MB
});

builder.Services.AddSingleton<IBadgeFactory, BadgeFactory>();
builder.Services.AddSingleton<IBadgeService, BadgeService>();
builder.Services.AddSingleton<ISvgService, SvgService>();
builder.Services.AddTransient<RecheckNotFoundJob>();
builder.Services.AddSingleton<RecheckNotFoundService>();
builder.Services.AddTransient<SymStoreService>();
builder.Services.AddTransient<ITracer, Tracer>();

builder.Services.AddHostedService<StartupService>();

builder.Services.AddScheduler();

builder.Services.AddFastEndpoints();

IConfigurationSection section = builder.Configuration.GetSection(nameof(ServiceConfig));

ServiceConfig? serviceConfig = section.Get<ServiceConfig>();

if (serviceConfig is null)
{
    Console.WriteLine("Missing service configuration, can't continue!");
    return;
}

builder.Services.Configure<ServiceConfig>(builder.Configuration.GetSection(nameof(ServiceConfig)));

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
                {
                    new(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String,
                        context.Options.ClaimsIssuer),
                    new(ClaimTypes.Name, context.Username, ClaimValueTypes.String,
                        context.Options.ClaimsIssuer)
                };

                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                context.Success();

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

Console.WriteLine("Initializing database connection");

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

await DB.InitAsync(serviceConfig.DatabaseName,
    MongoClientSettings.FromConnectionString(serviceConfig.ConnectionString));

Console.WriteLine("Running database migrations (if any)");

await DB.MigrateAsync();

Console.WriteLine("Creating index");

await DB.Index<SymbolsEntity>()
    .Key(a => a.IndexPrefix, KeyType.Text)
    .Key(a => a.FileName, KeyType.Text)
    .CreateAsync();

WebApplication? app = builder.Build().Setup();

app.Services.UseScheduler(scheduler =>
{
    scheduler
        .Schedule<RecheckNotFoundJob>()
        .DailyAtHour(3);
});

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

await app.RunAsync();