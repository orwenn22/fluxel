using System.Collections.Generic;
using fluxel.Models.Maps;
using fluxel.Models.Scores;
using fluXis.Online.API.Models.Scores;

namespace fluxel.Database.Extensions;

public static class ScoreExtensions
{
    public static APIScore ToAPI(this Score score, List<ScoreIncludes>? include = default)
    {
        var apiScore = new APIScore
        {
            ID = score.ID,
            User = score.APIUser,
            Time = score.TimeLong,
            Mode = score.Mode,
            Mods = score.Mods,
            PerformanceRating = score.PerformanceRating,
            TotalScore = score.TotalScore,
            Accuracy = score.Accuracy,
            Rank = score.Grade,
            MaxCombo = score.MaxCombo,
            FlawlessCount = score.FlawlessCount,
            PerfectCount = score.PerfectCount,
            GreatCount = score.GreatCount,
            AlrightCount = score.AlrightCount,
            OkayCount = score.OkayCount,
            MissCount = score.MissCount,
            ScrollSpeed = score.ScrollSpeed
        };

        if (include == null || include.Count == 0)
            return apiScore;

        if (include.Contains(ScoreIncludes.Map))
            apiScore.Map = score.APIMap;

        return apiScore;
    }

    public static bool MatchesVersion(this Score score, Map map) => score.MatchesVersion(map.SHA256Hash);
    public static bool MatchesVersion(this Score score, string? hash) => score.MapHash == hash;
}
