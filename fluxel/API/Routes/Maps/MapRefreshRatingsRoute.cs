using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Maps;

public class MapRefreshRatingsRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/map/:id/refresh-rate";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.User.IsDeveloper())
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, ResponseStrings.NoPermission);
            return;
        }

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

        var rating = map.RecalculateRating();
        MapHelper.QuickUpdate(map.ID, m => m.Rating = rating);
        await interaction.Reply(HttpStatusCode.OK, rating);
    }
}
