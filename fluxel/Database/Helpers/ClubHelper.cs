using System;
using System.Collections.Generic;
using System.Linq;
using fluxel.Models;
using fluxel.Models.Clubs;
using fluxel.Models.Scores;
using Midori.Utils;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class ClubHelper
{
    private static IMongoCollection<Club> clubs => MongoDatabase.GetCollection<Club>("clubs");
    private static IMongoCollection<ClubScore> scores => MongoDatabase.GetCollection<ClubScore>("club-scores");
    private static IMongoCollection<ClubClaim> claims => MongoDatabase.GetCollection<ClubClaim>("club-claims");
    private static IMongoCollection<ClubInvite> invites => MongoDatabase.GetCollection<ClubInvite>("club-invites");

    #region Clubs themselves

    public static List<Club> All => clubs.Find(m => true).ToList();

    public static Club? Get(long id) => clubs.Find(m => m.ID == id).FirstOrDefault();

    public static Club? ByTag(string tag) => clubs.Find(m => m.Tag == tag).FirstOrDefault();
    public static Club? ByName(string name) => clubs.Find(m => m.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

    public static void Add(Club club)
    {
        club.ID = CounterHelper.GetNext(CounterType.Club);
        clubs.InsertOne(club);
    }

    public static void Update(Club club) => clubs.ReplaceOne(m => m.ID == club.ID, club);

    public static Club? GetWhereUserIsMember(long userId) => clubs.Find(m => m.Members.Contains(userId)).FirstOrDefault();

    #endregion

    #region Scores

    public static List<ClubScore> GetScores(long clubId) => scores.Find(s => s.ClubID == clubId).ToList();

    public static List<ClubScore> GetScoresOnMap(long mapId) => scores.Find(s => s.MapID == mapId).ToList();

    public static ClubScore? GetScore(long clubId, long mapId, bool createIfNull = false)
    {
        var first = scores.Find(s => s.ClubID == clubId && s.MapID == mapId).FirstOrDefault();

        if (first == null && createIfNull)
        {
            first = new ClubScore
            {
                ClubID = clubId,
                MapID = mapId
            };

            scores.InsertOne(first);
        }

        return first;
    }

    public static void UpdateScore(ClubScore score)
    {
        if (score.PerformanceRating <= 0)
        {
            scores.DeleteOne(s => s.ID == score.ID);
            return;
        }

        scores.ReplaceOne(s => s.ID == score.ID, score);
    }

    #endregion

    #region Claims

    public static ClubClaim? GetClaim(long mapId, bool createIfNull = false)
    {
        var first = claims.Find(s => s.MapID == mapId).FirstOrDefault();

        if (first == null && createIfNull)
        {
            first = new ClubClaim { MapID = mapId };
            claims.InsertOne(first);
        }

        return first;
    }

    public static void UpdateClaim(ClubClaim claim)
    {
        if (claim.ClubID <= 0)
        {
            claims.DeleteOne(s => s.MapID == claim.MapID);
            return;
        }

        claims.ReplaceOne(s => s.MapID == claim.MapID, claim);
    }

    public static IEnumerable<ClubClaim> GetAllClaimed(long club)
        => claims.Find(c => c.ClubID == club).ToList();

    #endregion

    #region Invites

    private static List<ClubInvite> allInvites => invites.Find(x => true).ToList();

    public static ClubInvite CreateInvite(long club, long user)
    {
        var invite = new ClubInvite
        {
            InviteCode = createCode(),
            ClubID = club,
            UserID = user
        };

        invites.InsertOne(invite);
        return invite;
    }

    public static ClubInvite? GetInvite(string code) => invites.Find(x => x.InviteCode == code).FirstOrDefault();

    public static void RemoveForUser(long user) => invites.DeleteMany(x => x.UserID == user);

    private static string createCode()
    {
        var existingCodes = allInvites.Select(x => x.InviteCode).ToList();
        var code = "";

        while (existingCodes.Contains(code) || string.IsNullOrEmpty(code))
            code = RandomizeUtils.GenerateRandomString(7);

        return code;
    }

    #endregion
}
