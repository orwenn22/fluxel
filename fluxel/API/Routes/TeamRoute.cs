using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes;

public class TeamRoute : IFluxelAPIRoute
{
    public string RoutePath => "/team";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var devs = UserHelper.InGroup("dev");
        var staff = UserHelper.InGroup("purifier").Concat(UserHelper.InGroup("moderators")).OrderBy(x => x.ID).ToList();

        await interaction.Reply(HttpStatusCode.OK, new
        {
            devs = devs.Select(x => x.ToAPI()),
            staff = staff.DistinctBy(x => x.ID).Select(x => x.ToAPI())
        });
    }
}
