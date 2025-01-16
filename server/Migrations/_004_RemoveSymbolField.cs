using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _004_RemoveSymbolField : IMigration
{
    public Task UpgradeAsync()
    {
        return DB.Update<SymbolsEntity>()
            .Match(_ => true)
            .WithPipelineStage("{ $unset: \"Symbol\" }")
            .ExecutePipelineAsync();
    }
}