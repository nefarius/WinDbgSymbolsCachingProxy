using Coravel.Invocable;

using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Jobs;

/// <summary>
///     Periodic job checking upstream for every cached nonexistent symbol entry.
/// </summary>
public sealed class RecheckNotFoundJob : IInvocable
{
    private readonly ILogger<RecheckNotFoundJob> _logger;
    private readonly RecheckNotFoundService _recheckNotFoundService;

    public RecheckNotFoundJob(ILogger<RecheckNotFoundJob> logger, RecheckNotFoundService recheckNotFoundService)
    {
        _logger = logger;
        _recheckNotFoundService = recheckNotFoundService;
    }

    public async Task Invoke()
    {
        _logger.LogInformation("Running 404 re-check task");

        await _recheckNotFoundService.Run();

        _logger.LogInformation("Re-check task finished");
    }
}