using System;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Maps.Modding;

public class ModQueueVote
{
    [BsonElement("user")]
    public long UserID { get; init; }

    [BsonElement("approve")]
    public bool Approve { get; init; }

    public ModQueueVote(long userID, bool approve)
    {
        UserID = userID;
        Approve = approve;
    }

    [BsonConstructor]
    [Obsolete("BSON constructor.")]
    public ModQueueVote()
    {
    }
}
