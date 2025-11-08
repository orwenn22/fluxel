using System;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace fluxel.Models.Featured;

[JsonObject(MemberSerialization.OptIn)]
public class FeaturedArtistTrack
{
    [BsonId]
    public string InternalID { get; set; } = null!;

    [BsonIgnore]
    [JsonProperty("id")]
    public string SongID => InternalID.Split("/").Last();

    [BsonElement("name")]
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [BsonElement("length")]
    [JsonProperty("length")]
    public string Length { get; set; } = null!;

    [BsonElement("bpm")]
    [JsonProperty("bpm")]
    public string BPM { get; set; } = null!;

    [BsonElement("genre")]
    [JsonProperty("genre")]
    public string Genre { get; set; } = null!;

    public FeaturedArtistTrack(string artist, string album, string id)
    {
        InternalID = $"{artist}/{album}/{id}".ToLower();
    }

    [BsonConstructor]
    [Obsolete("BSON parsing")]
    public FeaturedArtistTrack()
    {
    }
}
