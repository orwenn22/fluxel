using System;
using System.Collections.Generic;
using System.Linq;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Maps;
using fluxel.Models.Users;
using fluxel.Utils;
using fluXis.Online.API.Models.Maps;
using fluXis.Online.API.Models.Users;
using fluXis.Scoring.Processing;
using fluXis.Utils;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Scores;

public class Score
{
    [BsonId]
    public long ID { get; set; }

    [BsonElement("user")]
    public long UserID { get; init; }

    [BsonIgnore]
    public User? User => Cache.Users.Get(UserID) ?? UserHelper.Get(UserID);

    [BsonIgnore]
    public APIUser APIUser => User?.ToAPI() ?? APIUser.CreateUnknown(UserID);

    [BsonElement("map")]
    public long MapID { get; init; }

    /// <summary>
    /// Hash of the map when the score was submitted.
    /// </summary>
    [BsonElement("hash")]
    public string MapHash { get; init; } = string.Empty;

    [BsonIgnore]
    public Map Map => Cache.Maps.Get(MapID) ?? MapHelper.Get(MapID) ?? new Map();

    [BsonIgnore]
    public APIMap APIMap => Map.ToAPI();

    [BsonElement("time")]
    public DateTimeOffset Time { get; init; } = DateTimeOffset.Now;

    [BsonIgnore]
    public long TimeLong => Time.ToUnixTimeSeconds();

    [BsonIgnore]
    public int Mode => Map.Mode;

    /// <summary>
    /// List of mods seperated by commas.
    /// </summary>
    [BsonElement("mods")]
    public string Mods { get; init; } = "";

    [BsonIgnore]
    public List<string> ModList => Mods.Split(',').ToList();

    [BsonElement("pr")]
    public double PerformanceRating { get; set; }

    [BsonElement("score")]
    public int TotalScore { get; set; }

    [BsonElement("accuracy")]
    public float Accuracy { get; set; }

    [BsonElement("grade")]
    public string Grade { get; set; } = null!;

    [BsonElement("combo")]
    public int MaxCombo { get; set; }

    [BsonElement("flawless")]
    public int FlawlessCount { get; set; }

    [BsonElement("perfect")]
    public int PerfectCount { get; set; }

    [BsonElement("great")]
    public int GreatCount { get; set; }

    [BsonElement("alright")]
    public int AlrightCount { get; set; }

    [BsonElement("okay")]
    public int OkayCount { get; set; }

    [BsonElement("miss")]
    public int MissCount { get; set; }

    [BsonElement("scrollspeed")]
    public float ScrollSpeed { get; set; }

    [BsonElement("replay")]
    public bool HasReplay { get; set; }

    [BsonIgnore]
    public RequestCache Cache { get; set; } = new();

    public void Recalculate()
    {
        Accuracy = this.CalculateAccuracy();
        TotalScore = this.CalculateScore();
        PerformanceRating = ScoreProcessor.CalculatePerformance(
            (float)Map.Rating,
            Accuracy,
            FlawlessCount,
            PerfectCount,
            GreatCount,
            AlrightCount,
            OkayCount,
            MissCount,
            ModList.Select(ModUtils.GetFromAcronym).ToList()
        );
        Grade = this.GetGrade();
    }
}

public enum ScoreIncludes
{
    Map
}
