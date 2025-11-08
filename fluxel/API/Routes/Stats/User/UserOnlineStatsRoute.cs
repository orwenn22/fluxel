using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes.Stats.User;

public class UserOnlineStatsRoute : IFluxelAPIRoute
{
    public string RoutePath => "/stats/users/online";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        await interaction.Reply(HttpStatusCode.OK, UserHelper.AllLogins.Select(x => new
        {
            time = x.Time,
            state = x.IsOnline
        }));
    }
}
