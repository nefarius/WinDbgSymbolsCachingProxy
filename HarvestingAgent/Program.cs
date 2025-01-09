using HarvestingAgent;

using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Debug Symbols Harvesting Agent";
});

builder.Services.Configure<ServiceConfig>(builder.Configuration.GetSection("ServiceConfig"));

LoggerProviderOptions.RegisterProviderOptions<
    EventLogSettings, EventLogLoggerProvider>(builder.Services);

builder.Services.AddHostedService<WindowsBackgroundService>();


IHost host = builder.Build();
host.Run();