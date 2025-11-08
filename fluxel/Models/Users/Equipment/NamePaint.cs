using System.Collections.Generic;
using fluxel.Models.Other;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Users.Equipment;

public class NamePaint
{
    [BsonId]
    public string ID { get; init; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("color")]
    public List<GradientColor> Colors { get; set; } = new();
}
