using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Modules;
using fluxel.Modules.Messages;
using fluxel.Tasks.Management;
using fluXis.Online.API.Models.Users;
using Midori.Networking;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace fluxel.Social;

public class NotificationsModule : IModule, IOnlineStateManager
{
    public static HttpConnectionManager<NotificationSocket> Sockets { get; private set; } = null!;

    public void OnLoad(ServerHost host)
    {
        fixInvalidOnlineStates();

        Sockets = host.Server.MapModule<NotificationSocket>("/notifications");
        host.Scheduler.Schedule(new CleanupOnlineStatesCronTask());
    }

    public void OnMessage(object data)
    {
        switch (data)
        {
            case UserCollectionMessage coll:
            {
                Sockets.Where(x => x.UserID == coll.UserID).ForEach(x => x.Client.CollectionUpdated(coll.CollectionID, coll.Added, coll.Changed, coll.Removed));
                break;
            }

            case UserAchievementMessage ach:
            {
                Sockets.Where(x => x.UserID == ach.UserID).ForEach(x => x.Client.RewardAchievement(ach.Achievement));
                break;
            }

            case UserNotificationMessage notif:
            {
                Sockets.Where(x => x.UserID == notif.UserID).ForEach(x => x.Client.NotificationReceived(notif.Notification));
                break;
            }

            case UserOnlineStateMessage onl:
            {
                var user = UserHelper.Get(onl.UserID) ?? throw new InvalidOperationException("Received online state update with non-existing user.");
                var followers = RelationHelper.GetFollowers(onl.UserID);

                Sockets.Where(s => followers.Contains(s.UserID))
                       .ForEach(s => s.Client.NotifyFriendStatus(user.ToAPI(), onl.Online));

                if (onl.Online)
                {
                    UserHelper.LogOnline(onl.UserID, true);

                    if (Sockets.Count(x => x.UserID == onl.UserID) > 1)
                    {
                        var connections = Sockets.Where(x => x.UserID == onl.UserID).ToList();
                        var lastConnection = connections.OrderBy(x => x.StartTime).First();
                        lastConnection.Client.Logout("Logged in from another location.");
                        // potentially force disconnect
                    }
                }
                else
                {
                    UserHelper.UpdateLocked(onl.UserID, u => u.LastLogin = DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    UserHelper.LogOnline(onl.UserID, false);
                }

                break;
            }
        }
    }

    private static void fixInvalidOnlineStates()
    {
        var online = UserHelper.LastOnlineLogs();
        online.ForEach(x => UserHelper.LogOnline(x, false));
    }

    long[] IOnlineStateManager.AllOnline => Sockets.Select(x => x.UserID).ToArray();

    bool IOnlineStateManager.IsOnline(long user) => Sockets.Any(x => x.UserID == user);

    APIActivity? IOnlineStateManager.GetActivity(long user)
    {
        var conn = Sockets.FirstOrDefault(x => x.UserID == user);
        if (conn?.Activity is null) return null;

        return new APIActivity
        {
            Name = conn.Activity.Value.name,
            Data = conn.Activity.Value.data
        };
    }
}
