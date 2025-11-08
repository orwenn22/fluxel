using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Maps;

public class MapRateVote
{
    [BsonId]
    public ObjectId ID { get; set; } = ObjectId.GenerateNewId();

    /// <summary>
    /// The user ID.
    /// </summary>
    [BsonElement("user")]
    public long UserID { get; init; }

    /// <summary>
    /// The map ID.
    /// </summary>
    [BsonElement("map")]
    public long MapID { get; init; }

    /// <summary>
    /// The vote value.
    /// </summary>
    [BsonElement("base")]
    public float BaseRating { get; init; }

    /// <summary>
    /// The vote value.
    /// </summary>
    [BsonElement("reading")]
    public float ReadingRating { get; init; }

    /// <summary>
    /// The vote value.
    /// </summary>
    [BsonElement("tracking")]
    public float TrackingRating { get; init; }

    /// <summary>
    /// The vote value.
    /// </summary>
    [BsonElement("perception")]
    public float PerceptionRating { get; init; }

    /// <summary>
    /// True when the vote was made by a purifier.
    /// Makes the vote count as 50 votes.
    /// </summary>
    [BsonElement("purifier")]
    public bool PurifierVote { get; init; }
}
