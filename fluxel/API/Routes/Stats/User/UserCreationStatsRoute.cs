using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes.Stats.User;

public class UserCreationStatsRoute : IFluxelAPIRoute
{
    public string RoutePath => "/stats/users/creation";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction) => await interaction.Reply(HttpStatusCode.OK, UserHelper.All.Where(u => u.CreatedAt > 0).Select(u => u.CreatedAt));
}
