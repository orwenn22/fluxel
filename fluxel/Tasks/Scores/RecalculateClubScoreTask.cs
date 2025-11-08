using System;
using System.Collections.Generic;
using System.Linq;
using fluxel.API.Components;
using fluxel.Database.Helpers;
using fluxel.Models.Scores;

namespace fluxel.Tasks.Scores;

public class RecalculateClubScoreTask : IBasicTask
{
    public string Name => $"RecalculateClubScore(map={mapID}, club={clubID})";

    private long mapID { get; }
    private long clubID { get; }

    public RecalculateClubScoreTask(long mapID, long clubID)
    {
        this.mapID = mapID;
        this.clubID = clubID;
    }

    public void Run()
    {
        var map = MapHelper.Get(mapID);

        if (map == null)
            throw new ArgumentException($"No map with id {mapID} was found!");

        var scores = ScoreHelper.FromMap(map, map.SHA256Hash);
        var cache = new RequestCache();

        // to make it a little bit faster
        foreach (var score in scores)
            score.Cache = cache;

        scores = scores.Where(s => s.User?.Club?.ID == clubID).ToList();
        scores = scores.OrderByDescending(s => s.TotalScore).ToList();

        // only take one score per user
        var uniqueScores = new List<Score>();

        foreach (var score in scores)
        {
            if (uniqueScores.Any(s => s.UserID == score.UserID))
                continue;

            uniqueScores.Add(score);
        }

        scores = uniqueScores;

        var clubScore = ClubHelper.GetScore(clubID, mapID, true)!;
        var idx = 0;

        // reset stats
        clubScore.TotalScore = 0;
        clubScore.PerformanceRating = 0;
        clubScore.Accuracy = 0;

        foreach (var score in scores)
        {
            clubScore.TotalScore += score.TotalScore;
            clubScore.PerformanceRating += score.PerformanceRating * Math.Pow(.9f, idx);
            clubScore.Accuracy += score.Accuracy;
            idx++;
        }

        clubScore.Accuracy /= scores.Count; // average accuracy
        ClubHelper.UpdateScore(clubScore);
    }
}
