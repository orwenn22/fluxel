using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Models.Users;
using Midori.Networking;

namespace fluxel.API.Routes.Leaderboards;

public class OverallRatingLeaderboardRoute : IFluxelAPIRoute
{
    public string RoutePath => "/leaderboards/overall";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var mode = interaction.GetIntQuery("mode") ?? 0;

        if (mode is > 8 or < 4)
            mode = 0;

        var all = interaction.Cache.Users.All.OrderByDescending(getRating).Take(100)
                             .Select(u => u.ToAPI(mode: mode, include: UserIncludes.Statistics))
                             .Where(u => u.Statistics!.OverallRating > 0).ToList();

        await interaction.Reply(HttpStatusCode.OK, all);

        double getRating(User u)
        {
            if (mode == 0)
                return u.OverallRating;

            var m = u.GetModeStatistics(mode);
            return m.OverallRating;
        }
    }
}
