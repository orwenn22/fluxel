using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Models.Scores;
using Midori.Networking;

namespace fluxel.API.Routes.Users;

public class UserScoresRoute : IFluxelAPIRoute
{
    public string RoutePath => "/user/:id/scores";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!interaction.Cache.Users.TryGet(id, out var user))
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.UserNotFound);
            return;
        }

        var includes = new List<ScoreIncludes> { ScoreIncludes.Map };

        await interaction.Reply(HttpStatusCode.OK, new
        {
            recent_scores = user.GetRecentScores().Select(x => x.ToAPI(includes)),
            best_scores = user.GetBestScores().Select(x => x.ToAPI(includes))
        });
    }
}
