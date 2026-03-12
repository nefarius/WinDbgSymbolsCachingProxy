using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _002_FileToFileName : IMigration
{
    /// <summary>
    /// Migrate the SymbolsEntity collection by renaming the "File" field to "FileName".
    /// </summary>
    /// <returns>A Task representing the update operation over the SymbolsEntity collection.</returns>
    public Task UpgradeAsync()
    {
        return DB.Default.Update<SymbolsEntity>()
            .Match(_ => true)
            .Modify(b => b.Rename("File", "FileName"))
            .ExecuteAsync();
    }
}