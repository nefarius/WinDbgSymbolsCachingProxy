namespace HarvestingAgent;

public sealed class HarvestingHostedService : IHostedService
{
    private readonly HarvesterRuntime _runtime;
    private readonly ILogger<HarvestingHostedService> _logger;

    public HarvestingHostedService(HarvesterRuntime runtime, ILogger<HarvestingHostedService> logger)
    {
        _runtime = runtime;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing symbol harvester");
        await _runtime.RebuildWatchersAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _runtime.Dispose();
        return Task.CompletedTask;
    }
}
