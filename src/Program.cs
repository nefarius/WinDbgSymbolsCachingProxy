using System.Net.Http.Headers;
using System.Reflection;

using FastEndpoints;

using MongoDB.Driver;
using MongoDB.Entities;

using Nefarius.Utilities.AspNetCore;

using Polly;
using Polly.Contrib.WaitAndRetry;

using WinDbgSymbolsCachingProxy.Services;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args).Setup();

builder.Services.AddSingleton<IBadgeFactory, BadgeFactory>();
builder.Services.AddSingleton<IBadgeService, BadgeService>();
builder.Services.AddSingleton<ISvgService, SvgService>();

builder.Services.AddFastEndpoints();

builder.Services.AddHttpClient("MicrosoftSymbolServer",
        client =>
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(
                Assembly.GetEntryAssembly()?.GetName().Name!,
                Assembly.GetEntryAssembly()?.GetName().Version!.ToString()));

            client.BaseAddress = new Uri("https://msdl.microsoft.com/");
        })
    .AddTransientHttpErrorPolicy(pb =>
        pb.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(3), 10)));

string? database = builder.Configuration.GetValue<string>("DatabaseName");
string? connectionString = builder.Configuration.GetValue<string>("ConnectionString");

await DB.InitAsync(database, MongoClientSettings.FromConnectionString(connectionString));

WebApplication? app = builder.Build().Setup();

app.UseAuthorization();
app.UseFastEndpoints();

await app.RunAsync();