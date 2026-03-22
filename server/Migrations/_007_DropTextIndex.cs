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
