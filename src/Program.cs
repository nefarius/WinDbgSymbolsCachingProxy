using System.Net.Http.Headers;
using System.Reflection;

using Coravel;

using FastEndpoints;

using MongoDB.Driver;
using MongoDB.Entities;

using Nefarius.Utilities.AspNetCore;

using Polly;
using Polly.Contrib.WaitAndRetry;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Jobs;
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

await DB.InitAsync(serviceConfig.DatabaseName,
    MongoClientSettings.FromConnectionString(serviceConfig.ConnectionString));

WebApplication? app = builder.Build().Setup();

app.Services.UseScheduler(scheduler =>
{
    scheduler
        .Schedule<RecheckNotFoundJob>()
        .DailyAtHour(3);
});

app.UseAuthorization();
app.UseFastEndpoints();

await app.RunAsync();