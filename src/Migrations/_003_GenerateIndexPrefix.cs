using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _003_GenerateIndexPrefix : IMigration
{
    public Task UpgradeAsync()
    {
        return DB.Update<SymbolsEntity>()
            .Match(_ => true)
            .WithPipelineStage("{$set:{IndexPrefix:{$concat:['$Symbol','/','$SymbolKey','/']}}}")
            .ExecutePipelineAsync();
    }
}