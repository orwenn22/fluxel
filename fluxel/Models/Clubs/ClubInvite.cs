using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace fluxel.Models.Clubs;

[JsonObject(MemberSerialization.OptIn)]
public class ClubInvite
{
    /// <summary>
    /// The unique invite code.
    /// </summary>
    [BsonId]
    public string InviteCode { get; set; } = "";

    /// <summary>
    /// The club the invite goes to.
    /// </summary>
    [BsonElement("club")]
    public long ClubID { get; set; }

    /// <summary>
    /// The user this invite is for.
    /// </summary>
    [BsonElement("user")]
    public long UserID { get; set; }
}
