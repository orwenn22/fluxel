using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace fluxel.Models.Other;

public class GradientColor
{
    [BsonElement("color")]
    [JsonProperty("color")]
    public string Color { get; set; } = "#FFFFFF";

    [BsonElement("pos")]
    [JsonProperty("position")]
    public double Position { get; set; }

    public static List<GradientColor> CreateHorizontal(string first, string second) => new()
    {
        new() { Position = 0, Color = first },
        new() { Position = 1, Color = second },
    };
}
