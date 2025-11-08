using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Maps;
using fluXis.Online.API.Models.Maps;
using Midori.Networking;

namespace fluxel.API.Routes.Clubs;

public class ClubClaimsRoute : IFluxelAPIRoute
{
    public string RoutePath => "/club/:id/claims";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        var club = ClubHelper.Get(id);

        if (club == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.ClubNotFound);
            return;
        }

        await interaction.Reply(HttpStatusCode.OK, ClubHelper.GetAllClaimed(id).Select(c =>
        {
            var score = ClubHelper.GetScore(c.ClubID, c.MapID);

            if (score is not null)
                score.Cache = interaction.Cache;

            return new
            {
                score = score?.ToAPI(),
                map = interaction.Cache.Maps.Get(c.MapID)?.ToAPI() ?? APIMap.CreateUnknown(c.MapID)
            };
        }).Where(c => c.map.Status >= (int)MapStatus.Pure));
    }
}

