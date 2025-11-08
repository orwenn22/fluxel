using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Notifications;
using fluXis.Online.API.Models.Notifications;
using fluXis.Online.API.Payloads.Invites;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Clubs.Invites;

public class CreateClubInviteRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/club/:id/invites";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!interaction.TryParseBody<CreateClubInvitePayload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        if (payload.UserID == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.MissingJsonField<CreateClubInvitePayload>(nameof(CreateClubInvitePayload.UserID)));
            return;
        }

        var club = ClubHelper.Get(id);

        if (club is null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.ClubNotFound);
            return;
        }

        if (club.OwnerID != interaction.UserID)
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, ResponseStrings.NoPermission);
            return;
        }

        var invite = ClubHelper.CreateInvite(club.ID, payload.UserID.Value);

        NotificationHelper.Create(new Notification(payload.UserID.Value, NotificationType.ClubInvite)
        {
            ClubInviteCode = invite.InviteCode
        });

        await interaction.Reply(HttpStatusCode.Created, invite.ToAPI());
    }
}
