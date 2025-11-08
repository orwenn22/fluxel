using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Users;

public class UserSocials
{
    [BsonElement("discord")]
    public string Discord { get; set; } = "";

    [BsonElement("twitter")]
    public string Twitter { get; set; } = "";

    [BsonElement("youtube")]
    public string YouTube { get; set; } = "";

    [BsonElement("twitch")]
    public string Twitch { get; set; } = "";
}
