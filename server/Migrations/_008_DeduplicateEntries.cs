using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Migrations;

/// <summary>
///     Removes duplicate (IndexPrefix, FileName) entries so a unique ascending index can be created.
///     For each group of duplicates, keeps the entry with the most recent LastAccessedAt (or the first if none).
/// </summary>
public class _008_DeduplicateEntries : IMigration
{
    /// <summary>
    /// Executes the migration that removes duplicate symbol records grouped by (IndexPrefix, FileName), keeping the most recently accessed record for each group.
    /// </summary>
    /// <returns>A Task that completes when the deduplication migration has finished.</returns>
    public Task UpgradeAsync()
    {
        return SymbolDedupMigrationHelper.DeduplicateSymbolsAsync(nameof(_008_DeduplicateEntries));
    }
}
