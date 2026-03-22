using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Migrations;

/// <summary>
///     Removes duplicate (IndexPrefix, FileName) entries so a unique ascending index can be created.
///     For each group of duplicates, keeps the entry with the most recent LastAccessedAt (or the first if none).
/// </summary>
public class _008_DeduplicateEntries : IMigration
{
    public Task UpgradeAsync()
    {
        return SymbolDedupMigrationHelper.DeduplicateSymbolsAsync(nameof(_008_DeduplicateEntries));
    }
}
