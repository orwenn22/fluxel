namespace fluxel;

public static class Program
{
    public static long MaintenanceTime { get; set; }

    public static void StartMaintenanceCountdown(long time)
    {
        MaintenanceTime = time;
        // NotificationConnections.ForEach(c => c.Client.DisplayMaintenance(time));
    }
}
