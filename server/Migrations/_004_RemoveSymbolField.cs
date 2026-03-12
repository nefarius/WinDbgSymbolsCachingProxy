using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

public class _004_RemoveSymbolField : IMigration
{
    /// <summary>
    /// Removes the "Symbol" field from all documents of type <c>SymbolsEntity</c> in the database.
    /// </summary>
    /// <returns>A task that completes when the "Symbol" field has been removed from all <c>SymbolsEntity</c> documents.</returns>
    public Task UpgradeAsync()
    {
        return DB.Default.Update<SymbolsEntity>()
            .Match(_ => true)
            .WithPipelineStage("{ $unset: \"Symbol\" }")
            .ExecutePipelineAsync();
    }
}