using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes.MapSets.Vote;

public class MapVoteGetRoute : IFluxelAPIRoute
{
    public string RoutePath => "/mapset/:id/votes";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        var set = MapSetHelper.Get(id);

        if (set == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapSetNotFound);
            return;
        }

        await interaction.Reply(HttpStatusCode.OK, set.GetVotes(interaction.IsAuthorized ? interaction.User.ID : 0));
    }
}
