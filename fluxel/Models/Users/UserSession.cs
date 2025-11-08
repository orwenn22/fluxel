using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace fluxel.Models.Users;

public class UserSession
{
    [BsonId]
    [JsonIgnore]
    public ObjectId ID { get; init; } = ObjectId.GenerateNewId();

    [BsonIgnore]
    [JsonProperty("id")]
    public string IDString => ID.ToString();

    [BsonElement("token")]
    [JsonIgnore]
    public string Token { get; init; } = string.Empty;

    [BsonElement("uid")]
    [JsonIgnore]
    public long UserID { get; init; }

    [BsonElement("ip")]
    [JsonIgnore]
    public string IP { get; init; } = string.Empty;

    [BsonElement("country")]
    [JsonProperty("country")]
    public string Country { get; init; } = string.Empty;

    [BsonElement("ua")]
    [JsonProperty("ua")]
    public string UserAgent { get; init; } = string.Empty;

    [BsonElement("last")]
    [JsonProperty("last")]
    public long LastActivity { get; set; }
}
