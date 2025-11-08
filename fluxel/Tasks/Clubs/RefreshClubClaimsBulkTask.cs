using System.Collections.Generic;
using System.Linq;
using fluxel.Database.Helpers;

namespace fluxel.Tasks.Clubs;

public class RefreshClubClaimsBulkTask : IBulkTask
{
    public IEnumerable<IBasicTask> GetTasks() => MapHelper.All.Select(m => new RefreshClubClaimTask(m.ID));
}
