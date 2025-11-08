using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes.Clubs;

public class ClubsRoute : IFluxelAPIRoute
{
    public string RoutePath => "/clubs";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction) => await interaction.Reply(HttpStatusCode.OK, ClubHelper.All.Select(x => x.ToAPI()));
}
