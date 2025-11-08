using System.Collections.Generic;
using System.Linq;
using fluxel.Database.Helpers;
using fluxel.Models.Clubs;
using fluxel.Models.Scores;
using fluxel.Models.Users;
using fluXis.Online.API.Models.Clubs;
using fluXis.Online.API.Models.Other;
using fluXis.Online.API.Models.Scores;
using fluXis.Online.API.Models.Users;

namespace fluxel.Database.Extensions;

public static class ClubExtensions
{
    public static APIClub ToAPI(this Club club, List<ClubIncludes>? include = default)
    {
        var c = new APIClub
        {
            ID = club.ID,
            Name = club.Name,
            Tag = club.Tag,
            IconHash = club.IconHash,
            BannerHash = club.BannerHash,
            MemberCount = club.Members.Count,
            Colors = club.Colors.Select(c => new APIGradientColor
            {
                Color = c.Color,
                Position = c.Position
            }).ToList()
        };

        // don't bother with the rest if we don't need it
        if (include == null || include.Count == 0)
            return c;

        if (include.Contains(ClubIncludes.Owner))
            c.Owner = club.Owner?.ToAPI(exclude: UserExclude.Club) ?? APIUser.CreateUnknown(club.OwnerID);
        if (include.Contains(ClubIncludes.JoinType))
            c.JoinType = club.JoinType;

        if (include.Contains(ClubIncludes.Members))
        {
            c.Members = club.MembersList.Select(m => m?.ToAPI(include: UserIncludes.LastLogin, exclude: UserExclude.Club) ?? APIUser.CreateUnknown(-1)).ToList();
            c.Members.RemoveAll(m => m.ID == -1);
        }

        if (include.Contains(ClubIncludes.Statistics))
        {
            var claims = ClubHelper.GetAllClaimed(club.ID).Count();
            var maps = MapHelper.PureCount;

            c.Statistics = new APIClubStatistics
            {
                OverallRating = club.OverallRating,
                TotalScore = club.TotalScore,
                Rank = club.GetRank(),
                TotalClaims = claims,
                ClaimPercentage = (claims / (double)maps) * 100
            };
        }

        return c;
    }

    public static APIClubScore ToAPI(this ClubScore score) => new()
    {
        Club = score.Cache.GetClub(score.ClubID)?.ToAPI() ?? ClubHelper.Get(score.ClubID)?.ToAPI() ?? APIClub.CreateUnknown(score.ClubID),
        MapID = score.MapID,
        TotalScore = score.TotalScore,
        PerformanceRating = score.PerformanceRating,
        Accuracy = score.Accuracy
    };

    public static APIClubInvite ToAPI(this ClubInvite invite) => new()
    {
        InviteCode = invite.InviteCode,
        Club = ClubHelper.Get(invite.ClubID)?.ToAPI() ?? APIClub.CreateUnknown(invite.ClubID),
        User = UserHelper.Get(invite.UserID)?.ToAPI() ?? APIUser.CreateUnknown(invite.UserID)
    };

    public static long GetRank(this Club club)
    {
        if (club.OverallRating == 0)
            return 0;

        var all = club.Cache.AllClubs;
        all.Sort((a, b) => a.OverallRating.CompareTo(b.OverallRating));
        all.Reverse();

        var rank = 0;

        foreach (var c in all)
        {
            rank++;
            if (c.ID == club.ID) break;
        }

        return rank;
    }
}
