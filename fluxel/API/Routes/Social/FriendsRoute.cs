using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Users;
using fluXis.Online.API.Models.Social;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Social;

public class FriendsRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/social/friends";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var following = RelationHelper.GetFollowing(interaction.UserID);

        // TODO: fix lobbies
        var friends = following.Select(interaction.Cache.Users.Get).OfType<User>();
        // var lobbies = following.Select(MultiplayerRoomManager.GetCurrentRoom).OfType<ServerMultiplayerRoom>();

        await interaction.Reply(HttpStatusCode.OK, new APIFriends
        {
            Users = friends.Select(x => x.ToAPI(interaction.UserID, include: UserIncludes.LastLogin)).ToList(),
            // Rooms = lobbies.Select(x => x.ToAPI()).ToList()
        });
    }
}
