using Microsoft.Extensions.Options;

namespace HarvestingAgent;

public class WindowsBackgroundService : BackgroundService
{
    private readonly IOptions<ServiceConfig> _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WindowsBackgroundService> _logger;
    private readonly FileSystemWatcher _watcher;

    public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger, IOptions<ServiceConfig> config,
        FileSystemWatcher watcher, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _config = config;
        _watcher = watcher;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("Server");

        _watcher.Created += WatcherOnCreated;

        void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        _watcher.EnableRaisingEvents = true;

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _watcher.EnableRaisingEvents = false;
    }
}