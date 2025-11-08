using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Helpers;
using fluXis.Online.API.Models.Notifications;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Social;

public class NotificationsListRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/social/notifications";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var notifications = NotificationHelper.ForUser(interaction.UserID);
        await interaction.Reply(HttpStatusCode.OK, new APINotificationList
        {
            Notifications = notifications.Select(x => x.ToAPI(interaction.Cache)).OfType<APINotification>().ToList(),
            LastRead = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });
    }
}
