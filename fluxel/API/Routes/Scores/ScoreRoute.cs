using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Scores;
using Midori.Networking;

namespace fluxel.API.Routes.Scores;

public class ScoreRoute : IFluxelAPIRoute
{
    public string RoutePath => "/score/:id";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        var score = ScoreHelper.Get(id);

        if (score == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.ScoreNotFound);
            return;
        }

        await interaction.Reply(HttpStatusCode.OK, score.ToAPI(new List<ScoreIncludes> { ScoreIncludes.Map }));
    }
}
