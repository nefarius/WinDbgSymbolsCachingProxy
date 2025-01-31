using System.Runtime.InteropServices;

using HarvestingAgent;

using Polly;
using Polly.Contrib.WaitAndRetry;

using Serilog;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    Log.Logger.Information("Configuring Windows Service");

    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "Debug Symbols Harvesting Agent";
    });
}

IConfigurationSection configSection = builder.Configuration.GetSection("ServiceConfig");
builder.Services.Configure<ServiceConfig>(configSection);

builder.Services.AddSerilog(lc => lc.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddHostedService<HarvestingBackgroundService>();

builder.Services.AddHttpClient("Server")
    .AddTransientHttpErrorPolicy(pb =>
        pb.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

IHost host = builder.Build();
host.Run();
