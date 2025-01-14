using System.Net.Http.Headers;
using System.Text;

using HarvestingAgent;

using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Debug Symbols Harvesting Agent";
});

IConfigurationSection configSection = builder.Configuration.GetSection("ServiceConfig");
builder.Services.Configure<ServiceConfig>(configSection);

LoggerProviderOptions.RegisterProviderOptions<
    EventLogSettings, EventLogLoggerProvider>(builder.Services);

builder.Services.AddSingleton<FileSystemWatcher>(_ =>
{
    ServiceConfig? config = configSection.Get<ServiceConfig>();

    if (config is null)
    {
        throw new InvalidOperationException("Configuration incomplete!");
    }

    FileSystemWatcher watcher = new(config.WatcherPath)
    {
        NotifyFilter = NotifyFilters.Attributes
                       | NotifyFilters.CreationTime
                       | NotifyFilters.DirectoryName
                       | NotifyFilters.FileName
                       | NotifyFilters.LastAccess
                       | NotifyFilters.LastWrite
                       | NotifyFilters.Size
    };

    // only watch those supported by the upload endpoint
    watcher.Filters.Add("*.exe");
    watcher.Filters.Add("*.dll");
    watcher.Filters.Add("*.pdb");

    return watcher;
});

builder.Services.AddHostedService<WindowsBackgroundService>();
builder.Services.AddHttpClient("Server", client =>
{
    ServiceConfig? config = configSection.Get<ServiceConfig>();

    if (config is null)
    {
        throw new InvalidOperationException("Configuration incomplete!");
    }

    client.BaseAddress = config.ServerUrl;

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        "Basic",
        Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{config.Authentication.Username}:{config.Authentication.Password}")));
});

IHost host = builder.Build();
host.Run();