using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Models.Maps;
using Midori.Networking;

namespace fluxel.API.Routes.MapSets;

public class MapSetBundledRoute : IFluxelAPIRoute
{
    public string RoutePath => "/mapsets/bundled";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var sets = ServerHost.Configuration.BundledSets.Select(interaction.Cache.MapSets.Get).OfType<MapSet>();
        await interaction.Reply(HttpStatusCode.OK, sets.Select(x => x.ToAPI()));
    }
}
