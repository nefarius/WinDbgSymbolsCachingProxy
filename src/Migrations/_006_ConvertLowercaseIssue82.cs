using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

/// <summary>
///     Migration to mitigate https://github.com/nefarius/WinDbgSymbolsCachingProxy/issues/82
/// </summary>
public class _006_ConvertLowercaseIssue82 : IMigration
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
        
        // convert SymbolKey
        await DB.Update<SymbolsEntity>()
            .Match(_ => true)
            .WithPipelineStage("{ $set: { SymbolKey: { $toLower: '$SymbolKey' } } }")
            .ExecutePipelineAsync();
    }
}