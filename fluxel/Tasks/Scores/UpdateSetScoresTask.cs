using System;
using System.Collections.Generic;
using System.Linq;
using fluxel.Database.Helpers;
using fluxel.Tasks.Users;
using Midori.Logging;

namespace fluxel.Tasks.Scores;

public class UpdateSetStatusBulkTask : IBulkTask
{
    private long id { get; }

    public UpdateSetStatusBulkTask(long id)
    {
        this.id = id;
    }

    public IEnumerable<IBasicTask> GetTasks()
    {
        var set = MapSetHelper.Get(id);

        if (set is null)
            return Array.Empty<IBasicTask>();

        var scores = set.MapsList.SelectMany(ScoreHelper.FromMap).ToList();
        var users = scores.Select(s => s.UserID).Distinct().ToList();
        Logger.Log($"Recalculating scores for {string.Join(", ", users)}. (status is {set.Status})");
        return users.Select(u => new RecalculateUserTask(u));
    }
}
