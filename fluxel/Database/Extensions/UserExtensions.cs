using System;
using System.Collections.Generic;
using System.Linq;
using fluxel.Database.Helpers;
using fluxel.Models.Scores;
using fluxel.Models.Users;
using fluxel.Models.Users.Equipment;
using fluXis.Online.API.Models.Other;
using fluXis.Online.API.Models.Users;
using fluXis.Online.API.Models.Users.Equipment;
using osu.Framework.Extensions.EnumExtensions;

namespace fluxel.Database.Extensions;

public static class UserExtensions
{
    public static APIUser ToAPI(this User user, long reqID = -1, int mode = 0, UserIncludes include = 0, UserExclude exclude = 0)
    {
        var u = new APIUser
        {
            ID = user.ID,
            SteamID = user.SteamID,
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarHash = user.AvatarHash,
            BannerHash = user.BannerHash,
            HasAnimatedAvatar = user is { HasAnimatedAvatar: true, IsSupporter: true },
            HasAnimatedBanner = user is { HasAnimatedBanner: true, IsSupporter: true },
            AboutMe = user.AboutMe,
            Pronouns = user.Pronouns,
            NamePaint = user.GetPaint()?.ToAPI(),
            CountryCode = user.CountryCode,
            Groups = user.Groups,
            Club = exclude.HasFlagFast(UserExclude.Club) ? null : user.Club?.ToAPI(),
            IsOnline = user.IsOnline,
            IsSupporter = user.IsSupporter
        };

        if (u.IsOnline)
        {
            var act = ServerHost.Instance.OnlineStates?.GetActivity(u.ID);
            if (act != null) u.Activity = act;
        }

        if (include.HasFlagFast(UserIncludes.CreatedAt))
            u.CreatedAt = user.CreatedAt;
        if (include.HasFlagFast(UserIncludes.LastLogin))
            u.LastLogin = user.LastLogin;
        if (include.HasFlagFast(UserIncludes.Email))
            u.Email = user.Email;
        if (include.HasFlagFast(UserIncludes.Flags))
            u.Flags = (long)user.BanFlags;

        if (include.HasFlagFast(UserIncludes.Following) && reqID >= 0)
            u.Following = RelationHelper.GetFollowState(reqID, user.ID);

        if (include.HasFlagFast(UserIncludes.Socials))
        {
            u.Socials = new APIUserSocials
            {
                Twitter = user.Socials.Twitter,
                Twitch = user.Socials.Twitch,
                YouTube = user.Socials.YouTube,
                Discord = user.Socials.Discord
            };
        }

        if (include.HasFlagFast(UserIncludes.Statistics))
        {
            var stats = new APIUserStatistics
            {
                MaxCombo = user.MaxCombo,
                RankedScore = user.RankedScore,
                OverallAccuracy = user.OverallAccuracy,
                CountryRank = user.GetCountryRank(mode),
                GlobalRank = user.GetGlobalRank(mode)
            };

            if (mode != 0)
            {
                var m = user.GetModeStatistics(mode);
                stats.OverallRating = m.OverallRating;
                stats.PotentialRating = m.PotentialRating;
            }
            else
            {
                stats.OverallRating = user.OverallRating;
                stats.PotentialRating = user.PotentialRating;
            }

            u.Statistics = stats;
        }

        return u;
    }

    public static APINamePaint ToAPI(this NamePaint paint) => new()
    {
        ID = paint.ID,
        Name = paint.Name,
        Colors = paint.Colors.Select(c => new APIGradientColor()
        {
            Color = c.Color,
            Position = c.Position
        }).ToList()
    };

    public static NamePaint? GetPaint(this User user)
    {
        if (string.IsNullOrEmpty(user.Paint))
            return null;

        var paint = UserEquipmentHelper.Get(user.Paint);
        return paint;
    }

    public static bool IsDeveloper(this User user) => user.Groups.Any(g => g.ID == "dev");
    public static bool IsPurifier(this User user) => user.IsDeveloper() || user.Groups.Any(g => g.ID == "purifier");
    public static bool IsModerator(this User user) => user.IsDeveloper() || user.Groups.Any(g => g.ID == "moderators");

    public static List<Score> GetRecentScores(this User user, List<Score>? scores = null, int? mode = null)
    {
        scores ??= ScoreHelper.GetByUser(user.ID);

        var maps = user.Cache.Maps;
        var sets = user.Cache.MapSets;
        maps.EnsureAll();
        sets.EnsureAll();

        var recent = new List<Score>();

        foreach (var score in scores.OrderByDescending(s => s.Time))
        {
            score.Cache = user.Cache;

            var map = maps.Get(score.MapID);

            if (map is null || !score.MatchesVersion(map))
                continue;

            if (mode > 0 && map.Mode != mode)
                continue;

            if (recent.Any(s => s.MapID == score.MapID) || !map.AllowScores())
                continue;

            recent.Add(score);

            if (recent.Count == 30)
                break;
        }

        return recent.ToList();
    }

    public static List<Score> GetBestScores(this User user, List<Score>? scores = null, int? mode = null)
    {
        scores ??= ScoreHelper.GetByUser(user.ID);

        var maps = user.Cache.Maps;
        var sets = user.Cache.MapSets;
        maps.EnsureAll();
        sets.EnsureAll();

        var best = new List<Score>();

        foreach (var score in scores.OrderByDescending(s => s.PerformanceRating))
        {
            score.Cache = user.Cache;

            if (!maps.TryGet(score.MapID, out var map) || !score.MatchesVersion(map))
                continue;

            if (mode > 0 && map.Mode != mode)
                continue;

            if (best.Any(s => s.MapID == score.MapID) || !map.AllowScores())
                continue;

            best.Add(score);
        }

        return best.Take(50).ToList();
    }

    public static double CalculateOverallRating(List<Score> best)
    {
        var ovr = 0d;
        var count = 0;

        foreach (var score in best)
        {
            ovr += score.PerformanceRating * Math.Pow(.9f, count);
            count++;
        }

        return Math.Round(ovr, 2);
    }

    public static double CalculatePotentialRating(List<Score> best, List<Score> recent)
    {
        var b = best.Take(30).Sum(score => score.PerformanceRating);
        var r = recent.Take(10).Sum(score => score.PerformanceRating);
        return Math.Round((b + r) / 40f, 2);
    }

    public static double CalculateAccuracy(this User user, List<Score> scores)
    {
        double acc = 0;
        var count = 0;

        var maps = user.Cache.Maps;
        var sets = user.Cache.MapSets;
        maps.EnsureAll();
        sets.EnsureAll();

        foreach (var score in scores)
        {
            score.Cache = user.Cache;

            if (!maps.TryGet(score.MapID, out var map) || !score.MatchesVersion(map))
                continue;

            if (!map.AllowScores())
                continue;

            acc += Math.Round(score.Accuracy, 2);
            count++;
        }

        if (count == 0)
            return 0;

        return acc / count;
    }

    public static int CalculateMaxCombo(this User _, List<Score> scores)
        => (from score in scores where score.Map.ID != 0 select score.MaxCombo).Prepend(0).Max();

    public static long CalculateRankedScore(this User _, List<Score> scores)
        => scores.Where(score => score.Map.ID != 0).Sum(score => (long)score.TotalScore);

    public static bool HasFlag(this User user, UserBanFlag banFlag) => (user.BanFlags & banFlag) == banFlag;

    public static UserBanFlag[] GetFlags(this User user) => Enum.GetValues(typeof(UserBanFlag)).Cast<UserBanFlag>().Where(user.HasFlag).ToArray();
}
