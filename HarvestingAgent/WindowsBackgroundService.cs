using Microsoft.Extensions.Options;

namespace HarvestingAgent;

public class WindowsBackgroundService : BackgroundService
{
    private readonly IOptions<ServiceConfig> _config;
    private readonly ILogger<WindowsBackgroundService> _logger;

    public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger, IOptions<ServiceConfig> config)
    {
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}