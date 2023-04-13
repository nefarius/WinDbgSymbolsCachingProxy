namespace WinDbgSymbolsCachingProxy.Services;

public sealed class StartupService : BackgroundService
{
    private readonly RecheckNotFoundService _recheckNotFoundService;
    private readonly ILogger _logger;

    public StartupService(RecheckNotFoundService recheckNotFoundService, ILogger logger)
    {
        _recheckNotFoundService = recheckNotFoundService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Running 404 re-check");
        
        await _recheckNotFoundService.Run(stoppingToken);
        
        _logger.LogInformation("Re-check finished");
    }
}