using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Relations;

public class FollowRelation
{
    [BsonId]
    [UsedImplicitly]
    public ObjectId ID { get; set; } = ObjectId.GenerateNewId();

    [BsonElement("follower")]
    public long FollowerID { get; init; }

    [BsonElement("followee")]
    public long FolloweeID { get; init; }
}
