using System;
using fluxel.Database.Helpers;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.OAuth;

public class OAuthToken
{
    [BsonId]
    public string AccessToken { get; init; } = SessionHelper.GenerateToken();

    [BsonElement("user")]
    public long UserID { get; init; }

    [BsonElement("scopes")]
    public OAuthScopes[] Scopes { get; init; } = Array.Empty<OAuthScopes>();

    [BsonElement("expire")]
    public long ExpireTime { get; init; }
}
