using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _005_ConvertLowercase : IMigration
{
    public async Task UpgradeAsync()
    {
        // convert IndexPrefix
        await DB.Update<SymbolsEntity>()
            .Match(_ => true)
            .WithPipelineStage("{ $set: { IndexPrefix: { $toLower: '$IndexPrefix' } } }")
            .ExecutePipelineAsync();
        
        // convert FileName
        await DB.Update<SymbolsEntity>()
            .Match(_ => true)
            .WithPipelineStage("{ $set: { FileName: { $toLower: '$FileName' } } }")
            .ExecutePipelineAsync();
    }
}