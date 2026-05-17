using System.Diagnostics.CodeAnalysis;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Controls the automatic pruning of stale not-found placeholders during the 404 recheck background job.
/// </summary>
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class NotFoundCleanupOptions
{
    /// <summary>
    ///     When <see langword="true" /> (the default), stale not-found placeholders are deleted during the 404 recheck run.
    ///     Set to <see langword="false" /> to disable all automatic cleanup.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     A not-found placeholder is deleted when it has been requested by at least one client before but
    ///     <see cref="WinDbgSymbolsCachingProxy.Models.SymbolsEntity.LastAccessedAt" /> is older than this threshold.
    ///     Defaults to 180 days (~6 months).
    /// </summary>
    public TimeSpan InactiveAfter { get; set; } = TimeSpan.FromDays(180);

    /// <summary>
    ///     A not-found placeholder is deleted when it has <em>never</em> been accessed by a client
    ///     (i.e. <see cref="WinDbgSymbolsCachingProxy.Models.SymbolsEntity.LastAccessedAt" /> is <see langword="null" />) and
    ///     <see cref="WinDbgSymbolsCachingProxy.Models.SymbolsEntity.NotFoundAt" /> is older than this threshold.
    ///     Defaults to 180 days (~6 months).
    /// </summary>
    public TimeSpan UnusedNotFoundAfter { get; set; } = TimeSpan.FromDays(180);
}
