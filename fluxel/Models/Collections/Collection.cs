using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Collections;

public class Collection
{
    [BsonId]
    public ObjectId ID { get; init; } = ObjectId.GenerateNewId();

    [BsonElement("owner")]
    public long OwnerID { get; init; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("created")]
    public long CreatedAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    [BsonElement("updated")]
    public long LastUpdated { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    [BsonElement("maps")]
    public List<long> MapIDs { get; init; } = new();
}
