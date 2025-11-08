using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes.Maps;

public class MapRoute : IFluxelAPIRoute
{
    public string RoutePath => "/map/:id";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        var map = MapHelper.Get(id);

        if (map == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapNotFound);
            return;
        }

        await interaction.Reply(HttpStatusCode.OK, map.ToAPI());
    }
}
