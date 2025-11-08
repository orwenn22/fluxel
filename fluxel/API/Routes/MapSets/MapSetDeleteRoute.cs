using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Maps;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.MapSets;

public class MapSetDeleteRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/mapset/:id";
    public HttpMethod Method => HttpMethod.Delete;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (interaction.User.HasMfa && !interaction.HasValidMfa)
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "mfa-required");
            return;
        }

        var set = MapSetHelper.Get(id);

        if (set == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapSetNotFound);
            return;
        }

        if (set.CreatorID != interaction.User.ID && !interaction.User.IsModerator())
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "You are not the creator of this mapset.");
            return;
        }

        if (set.Status >= MapStatus.Pure)
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "You cannot delete a purified mapset.");
            return;
        }

        MapSetHelper.Delete(set.ID);
        await interaction.Reply(HttpStatusCode.OK, "Mapset successfully deleted.");
    }
}
