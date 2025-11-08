using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluXis.Online.API.Models.Users;
using Midori.Networking;

namespace fluxel.API.Routes.Leaderboards.Users.Maps;

public class UploadedMapCountLeaderboard : IFluxelAPIRoute
{
    public string RoutePath => "/leaderboards/users/maps/uploaded";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var sets = MapSetHelper.All;
        sets.ForEach(set => set.Cache = interaction.Cache);

        var byUser = sets.GroupBy(x => x.CreatorID)
                         .OrderByDescending(x => x.Count()).Take(25);

        await interaction.Reply(HttpStatusCode.OK, byUser.Select(x => new
        {
            user = interaction.Cache.Users.Get(x.Key)?.ToAPI() ?? APIUser.CreateUnknown(x.Key),
            maps = x.Select(s => s.ToAPI()).ToList()
        }));
    }
}
