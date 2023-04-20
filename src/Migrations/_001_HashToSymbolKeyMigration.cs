using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _001_HashToSymbolKeyMigration : IMigration
{
    public Task UpgradeAsync()
    {
        return DB.Update<SymbolsEntity>()
            .Match(_ => true)
            .Modify(b => b.Rename("Hash", "SymbolKey"))
            .ExecuteAsync();
    }
}