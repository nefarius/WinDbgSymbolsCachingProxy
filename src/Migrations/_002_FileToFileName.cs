using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _002_FileToFileName : IMigration
{
    public Task UpgradeAsync()
    {
        return DB.Update<SymbolsEntity>()
            .Match(_ => true)
            .Modify(b => b.Rename("File", "FileName"))
            .ExecuteAsync();
    }
}