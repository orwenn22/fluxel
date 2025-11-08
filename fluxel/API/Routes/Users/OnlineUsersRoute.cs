using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluXis.Online.API.Models.Users;
using fluXis.Online.API.Responses.Users;
using Midori.Networking;

namespace fluxel.API.Routes.Users;

public class OnlineUsersRoute : IFluxelAPIRoute
{
    public string RoutePath => "/users/online";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var users = new List<APIUser>();

        foreach (var uid in GlobalStatistics.OnlineUsers)
        {
            var user = interaction.Cache.Users.Get(uid);
            users.Add(user?.ToAPI() ?? APIUser.CreateUnknown(uid));
        }

        await interaction.Reply(HttpStatusCode.OK, new OnlineUsers(users.Count(x => x.ID != 0), users));
    }
}
