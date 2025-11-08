using System;
using System.Linq;
using fluxel.Database.Helpers;

namespace fluxel.Tasks.Management;

public class CleanupOnlineStatesCronTask : ICronTask
{
    public string Name => "CleanUpOnlineStates";

    public int Hour => 00;
    public int Minute => 00;
    public bool Valid { get; set; }

    public void Run()
    {
        var current = DateTimeOffset.Now.ToUnixTimeSeconds();
        const int duration = 2 * 24 * 60 * 60;

        foreach (var login in UserHelper.AllLogins.Where(login => login.Time < current - duration))
            UserHelper.ClearLogin(login);
    }
}
