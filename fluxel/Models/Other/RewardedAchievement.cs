using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Other;

public class RewardedAchievement
{
    [BsonId]
    public ObjectId ID { get; set; } = ObjectId.GenerateNewId();

    [BsonElement("achievement")]
    public string AchievementID { get; set; } = null!;

    [BsonElement("user")]
    public long UserID { get; set; }

    [BsonElement("timestamp")]
    public long Timestamp { get; set; }

    public RewardedAchievement(string achievementID, long userID)
    {
        AchievementID = achievementID;
        UserID = userID;
        Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    [BsonConstructor]
    public RewardedAchievement()
    {
    }
}
