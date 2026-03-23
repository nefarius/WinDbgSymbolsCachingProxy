using System.Diagnostics;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;

using Serilog;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

/// <summary>
///     Shared duplicate removal for symbol rows keyed by (IndexPrefix, FileName), keeping the most recently accessed row.
/// </summary>
internal static class SymbolDedupMigrationHelper
{
    private const int DeleteBatchSize = 10_000;

    /// <summary>
    ///     Finds groups with more than one row per (IndexPrefix, FileName) and deletes extras in batches via
    ///     <see cref="DB.DeleteAsync{T}(System.Collections.Generic.IEnumerable{object},System.Threading.CancellationToken)" />.
    ///     Removes duplicate SymbolsEntity documents that share the same IndexPrefix and FileName, keeping the document with
    ///     the most recent LastAccessedAt and deleting the others in batches.
    /// </summary>
    /// <param name="migrationStepName">A label used in log entries to identify this migration step.</param>
    public static async Task DeduplicateSymbolsAsync(string migrationStepName)
    {
        IMongoCollection<SymbolsEntity> collection = DB.Default.Collection<SymbolsEntity>();

        Log.Information("Migration {Migration}: scanning for duplicate (IndexPrefix, FileName) groups", migrationStepName);

        PipelineDefinition<SymbolsEntity, BsonDocument> pipeline = new BsonDocument[]
        {
            new("$sort", new BsonDocument("LastAccessedAt", -1)),
            new("$group", new BsonDocument
            {
                { "_id", new BsonDocument { { "IndexPrefix", "$IndexPrefix" }, { "FileName", "$FileName" } } },
                { "keepId", new BsonDocument("$first", "$_id") },
                { "allIds", new BsonDocument("$push", "$_id") },
                { "count", new BsonDocument("$sum", 1) }
            }),
            new("$match", new BsonDocument("count", new BsonDocument("$gt", 1)))
        };

        AggregateOptions options = new() { AllowDiskUse = true };
        using IAsyncCursor<BsonDocument> cursor = await collection.AggregateAsync(pipeline, options);

        List<object> batch = new(DeleteBatchSize);
        int totalDeleted = 0;
        int batchNumber = 0;
        Stopwatch sw = Stopwatch.StartNew();

        while (await cursor.MoveNextAsync())
        {
            foreach (BsonDocument group in cursor.Current)
            {
                string keepId = group["keepId"].ToString()!;
                foreach (BsonValue idVal in group["allIds"].AsBsonArray)
                {
                    string id = idVal.ToString()!;
                    if (id == keepId)
                    {
                        continue;
                    }

                    batch.Add(id);
                    if (batch.Count < DeleteBatchSize)
                    {
                        continue;
                    }

                    batchNumber++;
                    int size = batch.Count;
                    await DB.Default.DeleteAsync<SymbolsEntity>(batch);
                    totalDeleted += size;
                    Log.Information(
                        "Migration {Migration}: deleted batch {BatchNumber} ({BatchSize} documents), cumulative deleted {TotalDeleted}, elapsed {Elapsed}",
                        migrationStepName, batchNumber, size, totalDeleted, sw.Elapsed);
                    batch = new List<object>(DeleteBatchSize);
                }
            }
        }

        if (batch.Count > 0)
        {
            batchNumber++;
            int size = batch.Count;
            await DB.Default.DeleteAsync<SymbolsEntity>(batch);
            totalDeleted += size;
            Log.Information(
                "Migration {Migration}: deleted batch {BatchNumber} ({BatchSize} documents), cumulative deleted {TotalDeleted}, elapsed {Elapsed}",
                migrationStepName, batchNumber, size, totalDeleted, sw.Elapsed);
        }

        if (totalDeleted == 0)
        {
            Log.Information("Migration {Migration}: deduplication finished; no duplicate rows found, elapsed {Elapsed}",
                migrationStepName, sw.Elapsed);
        }
        else
        {
            Log.Information(
                "Migration {Migration}: deduplication finished; removed {TotalDeleted} duplicate rows in {BatchCount} batches, total time {Elapsed}",
                migrationStepName, totalDeleted, batchNumber, sw.Elapsed);
        }
    }
}
