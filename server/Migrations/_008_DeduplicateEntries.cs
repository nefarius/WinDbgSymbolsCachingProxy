using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

/// <summary>
///     Removes duplicate (IndexPrefix, FileName) entries so a unique ascending index can be created.
///     For each group of duplicates, keeps the entry with the most recent LastAccessedAt (or the first if none).
/// </summary>
public class _008_DeduplicateEntries : IMigration
{
    public async Task UpgradeAsync()
    {
        IMongoCollection<SymbolsEntity> collection = DB.Default.Collection<SymbolsEntity>();

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

        using IAsyncCursor<BsonDocument> cursor = await collection.AggregateAsync(pipeline);

        while (await cursor.MoveNextAsync())
        {
            foreach (BsonDocument group in cursor.Current)
            {
                string keepId = group["keepId"].ToString()!;
                List<string> idsToDelete = group["allIds"].AsBsonArray
                    .Select(id => id.ToString()!)
                    .Where(id => id != keepId)
                    .ToList();

                foreach (string id in idsToDelete)
                {
                    await DB.Default.DeleteAsync<SymbolsEntity>(id);
                }
            }
        }
    }
}
