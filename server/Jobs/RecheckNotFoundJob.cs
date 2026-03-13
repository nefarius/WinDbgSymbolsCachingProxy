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
    /// <summary>
    ///     Runs the 404 recheck: queries upstream for all cached not-found symbols and updates the cache when they become available.
    /// </summary>
    /// <returns>A task that completes when the recheck has finished.</returns>
    public async Task Invoke()
    {
        logger.LogInformation("Running 404 re-check task");

        await recheckNotFoundService.Run();

        logger.LogInformation("Re-check task finished");
    }
}