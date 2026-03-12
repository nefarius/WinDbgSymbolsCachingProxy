using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _003_GenerateIndexPrefix : IMigration
{
    /// <summary>
    /// Adds an IndexPrefix field to all SymbolsEntity documents by concatenating Symbol, '/', SymbolKey, and '/'.
    /// </summary>
    /// <returns>A Task that completes when the update pipeline has been executed.</returns>
    public Task UpgradeAsync()
    {
        return DB.Default.Update<SymbolsEntity>()
            .Match(_ => true)
            .WithPipelineStage("{$set:{IndexPrefix:{$concat:['$Symbol','/','$SymbolKey','/']}}}")
            .ExecutePipelineAsync();
    }
}