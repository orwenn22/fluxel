using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Clubs;

public class ClubClaim
{
    [BsonId]
    public long MapID { get; init; }

    [BsonElement("club")]
    public long ClubID { get; set; }
}
