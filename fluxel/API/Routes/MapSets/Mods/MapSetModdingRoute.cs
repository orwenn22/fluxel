using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes.MapSets.Mods;

public class MapSetModdingRoute : IFluxelAPIRoute
{
    public string RoutePath => "/mapset/:id/modding";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (MapSetHelper.Get(id) is null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapSetNotFound);
            return;
        }

        var actions = MapSetHelper.GetModActionsFromSet(id);
        await interaction.Reply(HttpStatusCode.OK, actions.Select(x => x.ToAPI(interaction.Cache)));
    }
}
