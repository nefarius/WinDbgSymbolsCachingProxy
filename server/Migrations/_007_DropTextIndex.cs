using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Migrations;

/// <summary>
///     Deduplicates SymbolsEntity on (IndexPrefix, FileName), then drops the old text index so the app can recreate it as a proper ascending unique index.
/// </summary>
public class _007_DropTextIndex : IMigration
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
                foreach (BsonValue idVal in group["allIds"].AsBsonArray)
                {
                    string id = idVal.ToString()!;
                    if (id != keepId)
                    {
                        await DB.Default.DeleteAsync<SymbolsEntity>(id);
                    }
                }
            }
        }

        try
        {
            await collection.Indexes.DropOneAsync("IndexPrefix_text_FileName_text");
        }
        catch (MongoCommandException)
        {
            // Index may not exist if this is a fresh database
        }
    }
}
