using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using fluxel.Authentication;
using fluxel.Models;
using fluxel.Models.Users;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class UserHelper
{
    private static IMongoCollection<User> users => MongoDatabase.GetCollection<User>("users");
    private static IMongoCollection<UserLogin> logins => MongoDatabase.GetCollection<UserLogin>("user-logins");

    public static List<User> All => users.Find(u => true).ToList();
    public static long Count => users.CountDocuments(u => true);

    public static List<UserLogin> AllLogins => logins.Find(u => true).ToList();

    public static User Add(string username, string email, string password, string? country)
    {
        var user = new User
        {
            ID = CounterHelper.GetNext(CounterType.User),
            Username = username,
            Email = email,
            Password = PasswordAuth.HashPassword(password),
            CountryCode = country
        };

        users.InsertOne(user);
        return user;
    }

    public static User? Get(long id) => users.Find(u => u.ID == id).FirstOrDefault();
    public static User? Get(string name) => users.Find(u => string.Equals(u.Username, name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

    public static bool TryGet(long id, [NotNullWhen(true)] out User? user)
    {
        user = Get(id);
        return user != null;
    }

    public static bool TryGet(string name, [NotNullWhen(true)] out User? user)
    {
        user = Get(name);
        return user != null;
    }

    public static IEnumerable<User> GetMany(IEnumerable<long> ids) => users.Find(u => ids.Contains(u.ID)).ToList();
    public static User? GetByDiscordID(ulong id) => users.Find(x => x.DiscordID == id).FirstOrDefault();

    #region Get (E-Mail)

    public static User? GetByEmail(string email)
        => users.Find(u => string.Equals(u.Email, email, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

    public static bool TryGetByEmail(string email, [NotNullWhen(true)] out User? user)
    {
        user = GetByEmail(email);
        return user != null;
    }

    public static User? GetByKoFiEmail(string email)
        => users.Find(u => string.Equals(u.KoFiEmail, email, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

    #endregion

    #region Updating

    private static ConcurrentDictionary<long, object> userLocks { get; } = new();

    public static User UpdateLocked(long id, Action<User>? action)
    {
        var lk = userLocks.GetOrAdd(id, _ => new object());

        lock (lk)
        {
            var user = Get(id);

            if (user is null)
                throw new ArgumentNullException(nameof(id), "No user with the provided ID found.");

            action?.Invoke(user);
            users.ReplaceOne(x => x.ID == user.ID, user);
            return user;
        }
    }

    #endregion

    public static List<User> InGroup(string group) => users.Find(u => u.GroupIDs.Contains(group)).ToList();

    public static bool UsernameExists(this string username) => users.Find(u => string.Equals(u.Username, username, StringComparison.CurrentCultureIgnoreCase)).Any();

    #region Online Logs

    private static readonly object online_log_lock = new();

    public static void LogOnline(long id, bool online)
    {
        lock (online_log_lock)
        {
            if (LastOnlineLogs().Contains(id) == online)
                return;

            logins.InsertOne(new UserLogin
            {
                Time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                UserID = id,
                IsOnline = online
            });
        }
    }

    public static void ClearLogin(UserLogin login) => logins.DeleteOne(x => x.ID == login.ID);

    public static List<long> LastOnlineLogs()
    {
        var l = logins.Find(_ => true).ToList();
        var u = l.GroupBy(x => x.UserID);
        var online = u.Where(x => x.LastOrDefault()?.IsOnline == true);
        return online.Select(x => x.Key).ToList();
    }

    #endregion
}
