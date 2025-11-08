using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Modules.Messages;
using fluXis.Online.API.Models.Maps;
using fluXis.Online.Collections;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.MapSets.Favorite;

public class MapSetUpdateFavoriteStateRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/mapset/:id/favorite";
    public HttpMethod Method => HttpMethod.Patch;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!interaction.TryParseBody<APIMapSetFavoriteState>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        var set = MapSetHelper.Get(id);

        if (set == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapSetNotFound);
            return;
        }

        var state = MapSetHelper.HasFavorite(interaction.UserID, set.ID);

        if (state != payload.Favorite)
        {
            if (payload.Favorite) MapSetHelper.AddFavorite(interaction.UserID, set.ID);
            else MapSetHelper.RemoveFavorite(interaction.UserID, set.ID);
        }

        await interaction.Reply(HttpStatusCode.OK, new APIMapSetFavoriteState { Favorite = MapSetHelper.HasFavorite(interaction.UserID, set.ID) });

        if (state != payload.Favorite)
        {
            if (payload.Favorite)
            {
                ServerHost.Instance.SendMessage(new UserCollectionMessage(
                    interaction.UserID,
                    "favorite",
                    set.MapsList.Select(m => new CollectionItem
                    {
                        ID = m.ID.ToString("X5"),
                        Type = CollectionItemType.Online,
                        Map = m.ToAPI()
                    }).ToList(),
                    new List<CollectionItem>(),
                    new List<string>()
                ));
            }
            else
            {
                ServerHost.Instance.SendMessage(new UserCollectionMessage(
                    interaction.UserID,
                    "favorite",
                    new List<CollectionItem>(),
                    new List<CollectionItem>(),
                    set.Maps.Select(m => m.ToString("X5")).ToList()
                ));
            }
        }
    }
}
