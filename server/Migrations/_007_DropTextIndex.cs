using MongoDB.Driver;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

/// <summary>
///     Drops the old text index before deduplication (cheaper deletes), then deduplicates SymbolsEntity on (IndexPrefix, FileName)
///     so the app can recreate a proper ascending unique index.
/// </summary>
public class _007_DropTextIndex : IMigration
{
    /// <summary>
    /// Drops the legacy "IndexPrefix_text_FileName_text" text index if it exists, then deduplicates SymbolsEntity records so a correct unique index can be recreated.
    /// </summary>
    /// <remarks>
    /// Missing-index errors during the drop are ignored; deduplication is performed by SymbolDedupMigrationHelper using this migration's name.
    /// </remarks>
    /// <returns>A task that completes when the migration has finished.</returns>
    public async Task UpgradeAsync()
    {
        IMongoCollection<SymbolsEntity> collection = DB.Default.Collection<SymbolsEntity>();

        try
        {
            await collection.Indexes.DropOneAsync("IndexPrefix_text_FileName_text");
        }
        catch (MongoCommandException)
        {
            // Index may not exist if this is a fresh database
        }

        await SymbolDedupMigrationHelper.DeduplicateSymbolsAsync(nameof(_007_DropTextIndex));
    }
}
