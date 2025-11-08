using fluxel.Database.Helpers;

namespace fluxel.Tasks.Users;

public class RecalculateUserTask : IBasicTask
{
    public string Name => $"RecalculateUser({id})";

    private long id { get; }

    public RecalculateUserTask(long id)
    {
        this.id = id;
    }

    public void Run() => UserHelper.UpdateLocked(id, u => u.Recalculate());
}
