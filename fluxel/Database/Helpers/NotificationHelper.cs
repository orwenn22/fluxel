using System.Collections.Generic;
using fluxel.Models.Notifications;
using fluxel.Tasks.Users;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class NotificationHelper
{
    private static IMongoCollection<Notification> notifications => MongoDatabase.GetCollection<Notification>("notifications");

    public static Notification Create(Notification notification)
    {
        notifications.InsertOne(notification);

        ServerHost.Instance.Scheduler.Schedule(new SendNotificationTask(notification));
        return notification;
    }

    public static List<Notification> ForUser(long id) => notifications.Find(x => x.UserID == id).ToList();
}
