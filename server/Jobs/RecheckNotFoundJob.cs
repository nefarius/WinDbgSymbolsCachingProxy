using Coravel.Invocable;

using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Jobs;

/// <summary>
///     Periodic job checking upstream for every cached nonexistent symbol entry.
/// </summary>
public sealed class RecheckNotFoundJob(
    ILogger<RecheckNotFoundJob> logger,
    RecheckNotFoundService recheckNotFoundService)
    : IInvocable
{
    public async Task Invoke()
    {
        logger.LogInformation("Running 404 re-check task");

        await recheckNotFoundService.Run();

        logger.LogInformation("Re-check task finished");
    }
}