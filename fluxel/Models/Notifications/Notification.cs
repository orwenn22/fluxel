using System;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Utils;
using fluXis.Online.API.Models.Clubs;
using fluXis.Online.API.Models.Notifications;
using fluXis.Online.API.Models.Notifications.Data;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;

namespace fluxel.Models.Notifications;

public class Notification
{
    [BsonId]
    public ObjectId ID { get; set; } = ObjectId.GenerateNewId();

    [BsonElement("user")]
    public long UserID { get; set; }

    [BsonElement("type")]
    public NotificationType Type { get; set; }

    [BsonElement("time")]
    public DateTime Time { get; set; } = DateTime.UtcNow;

    #region Extra Data

    [BsonElement("club-invite-code")]
    public string? ClubInviteCode { get; set; }

    #endregion

    public Notification(long id, NotificationType type)
    {
        UserID = id;
        Type = type;
    }

    [BsonConstructor]
    public Notification()
    {
    }

    public APINotification? ToAPI(RequestCache cache)
    {
        var notif = new APINotification
        {
            Type = Type,
            Time = Time.ToUnixSeconds()
        };

        switch (Type)
        {
            case NotificationType.ClubInvite:
            {
                if (ClubInviteCode is null) return null;

                var invite = ClubHelper.GetInvite(ClubInviteCode);
                if (invite is null) return null;

                notif.Data = JObject.FromObject(new ClubInviteNotification
                {
                    Club = cache.GetClub(invite.ClubID)?.ToAPI() ?? APIClub.CreateUnknown(invite.ClubID),
                    InviteCode = invite.InviteCode
                });

                break;
            }
        }

        return notif;
    }
}
