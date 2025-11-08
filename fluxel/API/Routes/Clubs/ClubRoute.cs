using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Clubs;
using Midori.Networking;

namespace fluxel.API.Routes.Clubs;

public class ClubRoute : IFluxelAPIRoute
{
    public string RoutePath => "/club/:id";
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

        await interaction.Reply(HttpStatusCode.OK, club.ToAPI(new List<ClubIncludes>
        {
            ClubIncludes.Owner,
            ClubIncludes.Members,
            ClubIncludes.JoinType,
            ClubIncludes.Statistics
        }));
    }
}
