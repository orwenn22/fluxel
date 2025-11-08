using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using fluxel.Models.Users;
using fluxel.Utils;
using Midori.Utils;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class SessionHelper
{
    private static IMongoCollection<UserSession> sessions => MongoDatabase.GetCollection<UserSession>("sessions");

    public static UserSession? Get(string token)
    {
        var sesh = sessions.Find(s => s.Token == token).FirstOrDefault();
        return sesh?.update();
    }

    public static List<UserSession> GetSessions(long id)
    {
        var sesh = sessions.Find(s => s.UserID == id).ToList();
        return sesh;
    }

    public static UserSession? GetSimilar(long id, string ip)
    {
        var sesh = sessions.Find(s => s.IP == ip && s.UserID == id && !s.UserAgent.ToLower().StartsWith("game")).FirstOrDefault();
        return sesh?.update();
    }

    public static async Task<UserSession> Create(long id, string ip, string userAgent)
    {
        var country = await IpUtils.GetCountryCode(ip);
        var token = GenerateToken();

        while (doesTokenExist(token))
            token = GenerateToken();

        var session = new UserSession
        {
            Token = token,
            UserID = id,
            IP = ip,
            Country = country ?? "xx",
            UserAgent = userAgent,
            LastActivity = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        await sessions.InsertOneAsync(session);
        return session;
    }

    public static void RemoveWhere(Expression<Func<UserSession, bool>> filter)
        => sessions.DeleteMany(filter);

    private static UserSession update(this UserSession session)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        session.LastActivity = now;
        sessions.ReplaceOne(s => s.Token == session.Token, session);
        return session;
    }

    public static string GenerateToken()
        => RandomizeUtils.GenerateRandomString(32, CharacterType.AllOfIt);

    private static bool doesTokenExist(string token) => sessions.Find(s => s.Token == token).FirstOrDefault() != null;
}
