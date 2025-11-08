using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Other;

public class StoredEvent
{
    [BsonId]
    public ObjectId ID { get; set; } = ObjectId.GenerateNewId();

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("banner")]
    public string BannerLocation { get; set; } = string.Empty;

    [BsonElement("library")]
    public string LibraryHash { get; set; } = string.Empty;

    [BsonElement("start")]
    public long StartTime { get; set; }

    [BsonElement("end")]
    public long EndTime { get; set; }
}
