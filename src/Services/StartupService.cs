using System.Diagnostics;

namespace WinDbgSymbolsCachingProxy.Services;

public sealed class StartupService : BackgroundService
{
    private readonly ILogger<StartupService> _logger;
    private readonly RecheckNotFoundService _recheckNotFoundService;

    public StartupService(RecheckNotFoundService recheckNotFoundService, ILogger<StartupService> logger)
    {
        _recheckNotFoundService = recheckNotFoundService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Stopwatch sw = Stopwatch.StartNew();

        _logger.LogInformation("Running 404 re-check");

        await _recheckNotFoundService.Run(stoppingToken);

        sw.Stop();

        _logger.LogInformation("Re-check finished after {Timespan}", sw.Elapsed);
    }
}