using System.Collections.Generic;
using System.Linq;
using fluxel.Models.Relations;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class RelationHelper
{
    private static IMongoCollection<FollowRelation> follows => MongoDatabase.GetCollection<FollowRelation>("follows");

    public static void StartFollow(long follower, long followee)
    {
        if (IsFollowing(follower, followee))
            return;

        // pov: self love
        if (follower == followee)
            return;

        var relation = new FollowRelation
        {
            FollowerID = follower,
            FolloweeID = followee
        };

        follows.InsertOne(relation);
    }

    public static void StopFollow(long follower, long followee) => follows.DeleteOne(x => x.FollowerID == follower && x.FolloweeID == followee);

    public static bool IsFollowing(long follower, long followee) => follows.Find(x => x.FollowerID == follower && x.FolloweeID == followee).Any();
    public static bool Mutual(long user1, long user2) => IsFollowing(user1, user2) && IsFollowing(user2, user1);

    public static List<long> GetFollowers(long followee) => follows.Find(x => x.FolloweeID == followee).ToList().Select(x => x.FollowerID).ToList();
    public static List<long> GetFollowing(long follower) => follows.Find(x => x.FollowerID == follower).ToList().Select(x => x.FolloweeID).ToList();
}
