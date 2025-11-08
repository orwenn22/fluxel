using fluxel.Models.Other;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class AchievementHelper
{
    private static IMongoCollection<RewardedAchievement> list => MongoDatabase.GetCollection<RewardedAchievement>("achievements");

    public static void Reward(string achievementID, long userID)
    {
        if (HasRewarded(achievementID, userID))
            return;

        list.InsertOne(new RewardedAchievement(achievementID, userID));
    }

    public static bool HasRewarded(string achievementID, long userID)
        => list.Find(m => m.AchievementID == achievementID && m.UserID == userID).Any();
}
