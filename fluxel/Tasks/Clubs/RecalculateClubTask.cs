using System;
using System.Collections.Generic;
using System.Linq;
using fluxel.Database.Helpers;
using fluxel.Models.Users;

namespace fluxel.Tasks.Clubs;

public class RecalculateClubTask : IBasicTask
{
    public string Name => $"RecalculateClub({id})";

    private long id { get; }

    public RecalculateClubTask(long id)
    {
        this.id = id;
    }

    public void Run()
    {
        var club = ClubHelper.Get(id);

        if (club == null)
            throw new ArgumentException($"No club with id {id} was found!");

        var members = club.MembersList;
        var scores = ClubHelper.GetScores(club.ID);

        club.OverallRating = overall(members);
        club.TotalScore = scores.Sum(s => s.TotalScore);
        ClubHelper.Update(club);
    }

    private static double overall(List<User> members)
    {
        var ovr = 0d;
        var count = 0;

        foreach (var member in members)
        {
            ovr += member.OverallRating * Math.Pow(.9f, count);
            count++;
        }

        return Math.Round(ovr, 2);
    }
}
