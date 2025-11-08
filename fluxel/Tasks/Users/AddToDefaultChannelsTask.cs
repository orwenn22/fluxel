using fluxel.Database.Helpers;

namespace fluxel.Tasks.Users;

public class AddToDefaultChannelsTask : IBasicTask
{
    public string Name => $"AddToDefaultChannels({id})";

    private long id { get; }

    public AddToDefaultChannelsTask(long id)
    {
        this.id = id;
    }

    public void Run()
    {
        ChatHelper.AddToChannel("general", id);
        ChatHelper.AddToChannel("mapping", id);
        ChatHelper.AddToChannel("off-topic", id);
    }
}
