using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes;

public class StatsRoute : IFluxelAPIRoute
{
    public string RoutePath => "/stats";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        await interaction.Reply(HttpStatusCode.OK, new
        {
            users = UserHelper.Count - 1,
            online = GlobalStatistics.Online,
            scores = ScoreHelper.Count,
            mapsets = MapSetHelper.Count
        });
    }
}
