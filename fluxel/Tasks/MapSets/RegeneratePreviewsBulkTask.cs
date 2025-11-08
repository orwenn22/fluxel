using System.Collections.Generic;
using fluxel.Database.Helpers;

namespace fluxel.Tasks.MapSets;

public class RegeneratePreviewsBulkTask : IBulkTask
{
    public IEnumerable<IBasicTask> GetTasks()
    {
        var sets = MapSetHelper.All;

        foreach (var set in sets)
            yield return new GeneratePreviewTask(set.ID);
    }
}
