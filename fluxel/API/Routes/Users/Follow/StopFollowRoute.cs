using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Users.Follow;

public class StopFollowRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/user/:id/follow";
    public HttpMethod Method => HttpMethod.Delete;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!UserHelper.TryGet(id, out var user))
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.UserNotFound);
            return;
        }

        if (!RelationHelper.IsFollowing(interaction.UserID, user.ID))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "You aren't even following them.");
            return;
        }

        RelationHelper.StopFollow(interaction.UserID, user.ID);

        if (user.ID == 0) // make fluxel follow the user back
            RelationHelper.StopFollow(0, interaction.UserID);

        await interaction.Reply(HttpStatusCode.OK, $"You are no longer following '{user.Username}'.");
    }
}
