using System;
using fluxel.Database.Helpers;

namespace fluxel.Tasks.Scores;

public class RecalculateScoreTask : IBasicTask
{
    public string Name => $"RecalculateScore({id})";

    private long id { get; }

    public RecalculateScoreTask(long id)
    {
        this.id = id;
    }

    public void Run()
    {
        var score = ScoreHelper.Get(id);

        if (score == null)
            throw new ArgumentException($"No score with id {id} was found!");

        score.Recalculate();
        ScoreHelper.Update(score);
    }
}
