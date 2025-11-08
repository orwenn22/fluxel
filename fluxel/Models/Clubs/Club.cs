using System.Collections.Generic;
using System.Linq;
using fluxel.API.Components;
using fluxel.Database.Helpers;
using fluxel.Models.Other;
using fluxel.Models.Users;
using fluXis.Online.API.Models.Clubs;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace fluxel.Models.Clubs;

[JsonObject(MemberSerialization.OptIn)]
public class Club
{
    [BsonId]
    public long ID { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = "";

    [BsonElement("tag")]
    public string Tag { get; set; } = "";

    [BsonElement("icon")]
    public string IconHash { get; set; } = "";

    [BsonElement("banner")]
    public string BannerHash { get; set; } = "";

    [BsonElement("join-type")]
    public ClubJoinType JoinType { get; set; }

    [BsonElement("color")]
    public List<GradientColor> Colors { get; set; } = new();

    [BsonElement("owner")]
    public long OwnerID { get; set; }

    [BsonElement("members")]
    public List<long> Members { get; set; } = new();

    [BsonElement("ovr")]
    public double OverallRating { get; set; }

    [BsonElement("score")]
    public long TotalScore { get; set; }

    [BsonIgnore]
    public List<User> MembersList => Members.Select(x => Cache.Users.Get(x) ?? UserHelper.Get(x)).OfType<User>().ToList();

    [BsonIgnore]
    public User? Owner => Cache.Users.Get(OwnerID) ?? UserHelper.Get(OwnerID);

    [BsonIgnore]
    public RequestCache Cache { get; set; } = new();
}

public enum ClubIncludes
{
    Owner,
    JoinType,
    Members,
    Statistics
}
