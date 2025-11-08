using System;
using System.Collections.Generic;
using fluxel.Models.OAuth;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class OAuthHelper
{
    private static IMongoCollection<OAuthToken> tokens => MongoDatabase.GetCollection<OAuthToken>("oauth");
    public static IEnumerable<OAuthToken> AllExpired => tokens.Find(x => x.ExpireTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToList();

    public static OAuthToken? GetToken(string accessToken)
    {
        var token = tokens.Find(x => x.AccessToken == accessToken).FirstOrDefault();

        if (token == null)
            return null;

        if (token.ExpireTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            tokens.DeleteOne(x => x.AccessToken == accessToken);
            return null;
        }

        return token;
    }

    public static void DeleteToken(OAuthToken token) => tokens.DeleteOne(x => x.AccessToken == token.AccessToken);

    /// <summary>
    /// Create a new OAuth token for the specified user.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="expireHours">The time in hours until the token expires.</param>
    /// <param name="scopes">The scopes to assign to the token.</param>
    /// <returns>The created token.</returns>
    public static OAuthToken CreateToken(long userId, long expireHours, params OAuthScopes[] scopes)
    {
        var token = new OAuthToken
        {
            UserID = userId,
            AccessToken = $"flx:{SessionHelper.GenerateToken()}",
            Scopes = scopes,
            ExpireTime = DateTimeOffset.UtcNow.AddHours(expireHours).ToUnixTimeSeconds()
        };

        tokens.InsertOne(token);

        return token;
    }
}
