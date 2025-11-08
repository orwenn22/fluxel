using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Users;

public class UserLogin
{
    [BsonId]
    public ObjectId ID { get; set; } = ObjectId.GenerateNewId();

    [BsonElement("time")]
    public long Time { get; set; }

    [BsonElement("user")]
    public long UserID { get; set; }

    /// <summary>
    /// true = logged in, false = logged out
    /// </summary>
    [BsonElement("online")]
    public bool IsOnline { get; set; }
}
