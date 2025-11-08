using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Maps;
using fluxel.Tasks.Scores;
using Midori.API.Components.Interfaces;
using Midori.Networking;
using Newtonsoft.Json;

namespace fluxel.API.Routes.MapSets;

public class MapSetMetadataPatchRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/mapset/:id/metadata";
    public HttpMethod Method => HttpMethod.Patch;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.User.IsModerator())
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, ResponseStrings.NoPermission);
            return;
        }

        if (!interaction.TryParseBody<Payload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        var set = MapSetHelper.Get(id);

        if (set is null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapSetNotFound);
            return;
        }

        var status = payload.Status != null;

        if (status)
            set.Status = payload.Status!.Value;

        MapSetHelper.Update(set);

        await interaction.Reply(HttpStatusCode.OK);

        if (status)
            ServerHost.Instance.Scheduler.Schedule(new UpdateSetStatusBulkTask(set.ID));
    }

    private class Payload
    {
        [JsonProperty("status")]
        public MapStatus? Status { get; set; }
    }
}
