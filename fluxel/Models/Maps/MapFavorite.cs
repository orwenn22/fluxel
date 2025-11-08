using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Maps;

public class MapFavorite
{
    [BsonId]
    public ObjectId ID { get; set; } = ObjectId.GenerateNewId();

    [BsonElement("mapset")]
    public long MapSetID { get; init; }

    [BsonElement("user")]
    public long UserID { get; init; }
}
