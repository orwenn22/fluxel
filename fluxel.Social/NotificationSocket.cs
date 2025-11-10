using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Modules.Messages;
using fluxel.WebSocket;
using fluXis.Online.Notifications;
using Newtonsoft.Json.Linq;

namespace fluxel.Social;

public class NotificationSocket : AuthenticatedSocket<INotificationServer, INotificationClient>, INotificationServer
{
    public (string name, JObject data)? Activity { get; private set; }

    protected override void OnOpen()
    {
        base.OnOpen();

        Client.Login(UserHelper.Get(UserID)?.ToAPI() ?? throw new ArgumentNullException(nameof(UserID), "how."));
        ServerHost.Instance.SendMessage(new UserOnlineStateMessage(UserID, true));

        if (CurrentUser.ForceNameChange)
            Client.ForceNameChange();
    }

    protected override void OnClose()
    {
        base.OnClose();
        ServerHost.Instance.SendMessage(new UserOnlineStateMessage(UserID, false));
    }

    public Task UpdateActivity(string name, JObject data)
    {
        Activity = (name, data);
        return Task.CompletedTask;
    }

    public Task UpdateNotificationUnread(long time)
    {
        UserHelper.UpdateLocked(UserID, x => x.LastNotificationRead = time);
        return Task.CompletedTask;
    }
}
