using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using fluxel.Models.Auth;
using Midori.Utils;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class AuthHelper
{
    #region Tokens

    private static Dictionary<long, MultifactorToken> tokens { get; } = new();

    public static string GenerateToken(long user)
    {
        var token = RandomizeUtils.GenerateRandomString(32, CharacterType.AllOfIt);
        var valid = DateTimeOffset.Now.AddMinutes(30).ToUnixTimeSeconds();

        tokens[user] = new MultifactorToken(token, valid);
        return token;
    }

    public static bool IsValidToken(long user, string token)
    {
        if (!tokens.TryGetValue(user, out var tk))
            return false;

        if (tk.ValidUntil < DateTimeOffset.Now.ToUnixTimeSeconds())
            return false;

        return tk.Token == token;
    }

    #endregion

    #region TOTP

    private static IMongoCollection<TimedCodeInfo> totpInfos => MongoDatabase.GetCollection<TimedCodeInfo>("totp");

    public static List<TimedCodeBackup> CreateTimeBased(long user, string key)
    {
        var totp = new TimedCodeInfo(user, key, Enumerable.Range(0, 12).Select(_ => TimedCodeBackup.Generate()).ToList());
        totpInfos.InsertOne(totp);
        return totp.BackupCodes;
    }

    public static bool HasCode(long user) => totpInfos.Find(x => x.UserID == user).FirstOrDefault() != null;

    public static bool TryGetTimeBased(long user, [NotNullWhen(true)] out TimedCodeInfo? totp)
    {
        totp = totpInfos.Find(x => x.UserID == user).FirstOrDefault();
        return totp != null;
    }

    #endregion

    #region Passkeys

    private static IMongoCollection<Passkey> passkeys => MongoDatabase.GetCollection<Passkey>("passkeys");
    private static List<Passkey> allPasskeys => passkeys.Find(_ => true).ToList();

    public static void Add(Passkey passkey)
        => passkeys.InsertOne(passkey);

    public static List<Passkey> GetByID(byte[] id)
        => allPasskeys.Where(x => x.ID.SequenceEqual(id)).ToList();

    public static List<Passkey> GetByUserID(long id)
        => allPasskeys.Where(x => x.UserID == id).ToList();

    public static List<Passkey> GetByHandle(byte[] handle)
        => allPasskeys.Where(x => x.UserHandle.SequenceEqual(handle)).ToList();

    public static void Update(Passkey key)
        => passkeys.ReplaceOne(x => x.ObjectID == key.ObjectID, key);

    #endregion

    private class MultifactorToken
    {
        public string Token { get; set; }
        public long ValidUntil { get; set; }

        public MultifactorToken(string token, long validUntil)
        {
            Token = token;
            ValidUntil = validUntil;
        }
    }
}
