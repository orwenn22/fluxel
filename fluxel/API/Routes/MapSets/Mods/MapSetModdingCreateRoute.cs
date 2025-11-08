using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Maps;
using fluXis.Online.API.Models.Maps.Modding;
using Midori.API.Components.Interfaces;
using Midori.Networking;
using Newtonsoft.Json;

namespace fluxel.API.Routes.MapSets.Mods;

public class MapSetModdingCreateRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/mapset/:id/modding";
    public HttpMethod Method => HttpMethod.Post;

    public IEnumerable<(string, string)> Validate(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out _))
            yield return ("_param", ResponseStrings.InvalidParameter("id", "long"));

        if (!interaction.TryParseBody<Payload>(out var payload))
            yield return ("_form", ResponseStrings.InvalidBodyJson);

        if (payload != null)
        {
            interaction.AddCache("payload", payload);

            if (payload.Type == null)
                yield return ("type", ResponseStrings.FieldRequired);
            else if (!Enum.IsDefined(typeof(APIModdingActionType), payload.Type))
                yield return ("type", $"'{payload.Type}' is not a valid action type.");
            else if (payload.Type is >= APIModdingActionType.Submitted)
                yield return ("type", $"'{payload.Type}' can not be created manually.");

            if (string.IsNullOrWhiteSpace(payload.Content))
                yield return ("content", ResponseStrings.FieldRequired);
        }
    }

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetCache<Payload>("payload", out var payload))
            throw new CacheMissingException("payload");

        if (!interaction.User.IsPurifier())
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "You need to be a purifier to create modding actions!");
            return;
        }

        var set = MapSetHelper.Get(interaction.GetLongParameter("id")!.Value);

        if (set is null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapSetNotFound);
            return;
        }

        if (set.Status != MapStatus.Pending)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Not in queue?");
            return;
        }

        if (!set.AddModdingEntry(payload.Type!.Value, interaction.UserID, out var error))
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, error);
            return;
        }

        var action = MapSetHelper.CreateModAction(set.ID, interaction.UserID, payload.Type.Value, payload.Content);
        await interaction.Reply(HttpStatusCode.Created, action.ToAPI(interaction.Cache));

        if (set.Status == MapStatus.Pure)
            Events.MapPure(set.ID);
    }

    public class Payload
    {
        [JsonProperty("type")]
        public APIModdingActionType? Type { get; set; }

        [JsonProperty("content")]
        public string? Content { get; set; }
    }
}
