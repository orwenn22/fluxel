using System;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluXis.Online.API.Payloads.Maps;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.MapSets.Vote;

public class MapVotePostRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/mapset/:id/votes";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!interaction.TryParseBody<MapVotePayload>(out var payload))
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

        var vote = Math.Clamp(payload.YourVote, -1, 1);
        set.SetVote(interaction.User.ID, vote);

        MapSetHelper.Update(set);

        await interaction.Reply(HttpStatusCode.OK, set.GetVotes(interaction.User.ID));
    }
}
