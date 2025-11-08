using fluxel.Models.Notifications;

namespace fluxel.Tasks.Users;

public class SendNotificationTask : IBasicTask
{
    public string Name => $"SendNotification({notification.UserID}, {notification.ID})";

    private Notification notification { get; }

    public SendNotificationTask(Notification notification)
    {
        this.notification = notification;
    }

    public void Run()
    {
        // TODO: fix
        // var conn = Program.NotificationConnections.Where(x => x.UserID == notification.UserID);
        // var cache = new RequestCache();
        // var notif = notification.ToAPI(cache);

        // foreach (var socket in conn)
            // socket.Client.NotificationReceived(notif);
    }
}
