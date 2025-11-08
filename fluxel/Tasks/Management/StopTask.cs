
namespace fluxel.Tasks.Management;

public class StopTask : IBasicTask
{
    public string Name => "Stop";

    public void Run()
    {
        /*Logger.Log("Shutting down...");

        try
        {
            // Program.API.Stop();
            DiscordBot.Stop().Wait();
            TaskRunner.Stop();
            MultiplayerRoomManager.StopThead();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed when shutting down!");
        }

        Program.Running = false;
        Environment.FailFast("Shutting down.");*/
    }
}
