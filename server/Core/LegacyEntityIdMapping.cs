using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     MongoDB.Entities v26 stores <see cref="Entity.ID" /> as a BSON string by default, while earlier versions used
///     BSON ObjectIds. Existing databases contain ObjectIds, so keep that representation for all entities.
/// </summary>
internal static class LegacyEntityIdMapping
{
    private static readonly object Gate = new();

    /// <summary>
    ///     Registers the ObjectId-backed ID mapping on the <see cref="Entity" /> base class. Must run before the first
    ///     use of any entity type (and before <c>DB.InitAsync</c>). Safe to call multiple times.
    /// </summary>
    public static void Register()
    {
        lock (Gate)
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(Entity)))
            {
                return;
            }

            BsonClassMap.RegisterClassMap<Entity>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapIdProperty(e => e.ID)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId))
                    .SetIdGenerator(StringObjectIdGenerator.Instance);
            });
        }
    }
}
