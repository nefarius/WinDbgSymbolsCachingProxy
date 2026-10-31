using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Server.Tests;

public class LegacyEntityIdMappingTests
{
    [Fact]
    public void LegacyObjectIdIsReadIntoStringId()
    {
        LegacyEntityIdMapping.Register();

        ObjectId id = ObjectId.GenerateNewId();
        BsonDocument doc = new() { { "_id", id }, { "Name", "Admin" } };

        RoleEntity role = BsonSerializer.Deserialize<RoleEntity>(doc);

        Assert.Equal(id.ToString(), role.ID);
        Assert.Equal("Admin", role.Name);
    }

    [Fact]
    public void StringIdIsWrittenAsObjectId()
    {
        LegacyEntityIdMapping.Register();

        ObjectId id = ObjectId.GenerateNewId();
        RoleEntity role = new() { ID = id.ToString(), Name = "Reader" };

        BsonDocument written = role.ToBsonDocument();

        Assert.Equal(BsonType.ObjectId, written["_id"].BsonType);
        Assert.Equal(id, written["_id"].AsObjectId);
    }
}
