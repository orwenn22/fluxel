using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes.Users;

public class UserFollowersRoute : IFluxelAPIRoute
{
    public string RoutePath => "/user/:id/followers";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!UserHelper.TryGet(id, out _))
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.UserNotFound);
            return;
        }

        var followerIDs = RelationHelper.GetFollowers(id);
        var followers = UserHelper.GetMany(followerIDs).Select(x => x.ToAPI());

        // sort with the most recent followers first
        followers = followers.OrderByDescending(x => followerIDs.IndexOf(x.ID));
        await interaction.Reply(HttpStatusCode.OK, followers);
    }
}
