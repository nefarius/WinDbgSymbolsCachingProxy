using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _005_ConvertLowercase : IMigration
{
    /// <summary>
    /// Converts the IndexPrefix and FileName fields of all SymbolsEntity documents in the default database to lowercase.
    /// </summary>
    public async Task UpgradeAsync()
    {
        // convert IndexPrefix
        await DB.Default.Update<SymbolsEntity>()
            .Match(_ => true)
            .WithPipelineStage("{ $set: { IndexPrefix: { $toLower: '$IndexPrefix' } } }")
            .ExecutePipelineAsync();
        
        // convert FileName
        await DB.Default.Update<SymbolsEntity>()
            .Match(_ => true)
            .WithPipelineStage("{ $set: { FileName: { $toLower: '$FileName' } } }")
            .ExecutePipelineAsync();
    }
}