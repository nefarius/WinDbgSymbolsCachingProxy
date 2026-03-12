using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _001_HashToSymbolKey : IMigration
{
    /// <summary>
    /// Renames the "Hash" field to "SymbolKey" on all documents in the SymbolsEntity collection.
    /// </summary>
    /// <returns>A task that completes when the update operation has finished.</returns>
    public Task UpgradeAsync()
    {
        return DB.Default.Update<SymbolsEntity>()
            .Match(_ => true)
            .Modify(b => b.Rename("Hash", "SymbolKey"))
            .ExecuteAsync();
    }
}