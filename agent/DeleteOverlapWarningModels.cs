namespace HarvestingAgent;

public sealed class DeleteOverlapWarningGroup
{
    public required string Path { get; init; }
    public required bool Recursive { get; init; }

    /// <summary>
    ///     Normalized upload filter key; matches <see cref="WatcherUploadFilterHelpers.NormalizeFiltersKey" /> /
    ///     <see cref="HarvesterRuntime" /> watcher grouping.
    /// </summary>
    public required string FiltersKey { get; init; }

    public required List<DeleteOverlapWarningServerEntry> Servers { get; init; }
}

public sealed class DeleteOverlapWarningServerEntry
{
    public required string Display { get; init; }
    public required bool DeleteEnabled { get; init; }
}

