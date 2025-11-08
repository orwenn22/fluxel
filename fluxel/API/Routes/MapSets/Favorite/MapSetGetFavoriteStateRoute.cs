using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluXis.Online.API.Models.Maps;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.MapSets.Favorite;

public class MapSetGetFavoriteStateRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/mapset/:id/favorite";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        var set = MapSetHelper.Get(id);

        if (set == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapSetNotFound);
            return;
        }

        await interaction.Reply(HttpStatusCode.OK, new APIMapSetFavoriteState { Favorite = MapSetHelper.HasFavorite(interaction.UserID, set.ID) });
    }
}
