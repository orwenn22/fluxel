using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Maps;
using fluXis.Online.API.Models.Users;
using fluXis.Online.Collections;
using Midori.Networking;

namespace fluxel.API.Routes.Users.Collections;

public class GetUserCollectionsRoute : IFluxelAPIRoute
{
    public string RoutePath => "/users/:id/collections";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetUserID("id", out var id))
            return;

        var favorite = new Collection
        {
            ID = "favorite",
            Name = "Favorite",
            Type = CollectionType.Favorite,
            Owner = UserHelper.Get(id)?.ToAPI() ?? APIUser.CreateUnknown(id),
            Items = MapSetHelper.AllFavoriteByUser(id)
                                .Select(MapSetHelper.Get).OfType<MapSet>()
                                .SelectMany(set => set.MapsList.Select(map => map.ToAPI(set: set, userid: interaction.UserID)))
                                .Select(x => new CollectionItem
                                {
                                    ID = x.ID.ToString("X5"),
                                    Type = CollectionItemType.Online,
                                    Map = x
                                }).ToList()
        };

        await interaction.Reply(HttpStatusCode.OK, new List<Collection> { favorite });
    }
}
