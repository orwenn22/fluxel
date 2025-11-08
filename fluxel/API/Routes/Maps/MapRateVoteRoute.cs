using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.API.Components.Interfaces;
using Midori.Networking;
using Newtonsoft.Json;

namespace fluxel.API.Routes.Maps;

public class MapRateVoteRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/map/:id/rate";
    public HttpMethod Method => HttpMethod.Post;

    public IEnumerable<(string, string)> Validate(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryParseBody<Payload>(out var payload))
            yield return ("_form", ResponseStrings.InvalidBodyJson);

        if (payload != null)
        {
            interaction.AddCache("payload", payload);

            if (payload.Base is null)
                yield return ("base", "Missing value.");
            else if (payload.Base is <= 0 or > 20)
                yield return ("base", "Has to be between above 0 and less or equal to 20.");

            if (payload.Reading is null)
                yield return ("read", "Missing value.");
            else if (payload.Reading is < 0 or > 5)
                yield return ("read", "Has to be 0-5.");

            if (payload.Tracking is null)
                yield return ("track", "Missing value.");
            else if (payload.Tracking is < 0 or > 5)
                yield return ("track", "Has to be 0-5.");

            if (payload.Perception is null)
                yield return ("percept", "Missing value.");
            else if (payload.Perception is < 0 or > 5)
                yield return ("percept", "Has to be 0-5.");
        }
    }

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!interaction.TryGetCache<Payload>("payload", out var payload))
            throw new CacheMissingException("payload");

        var map = MapHelper.Get(id);

        if (map == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapNotFound);
            return;
        }

        if (map.MapperID == interaction.User.ID && !interaction.User.IsDeveloper())
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.CannotRateOwnMap);
            return;
        }

        if (MapHelper.HasVoted(interaction.User.ID, id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.AlreadyVoted);
            return;
        }

        var purifier = interaction.User.IsPurifier();

        MapHelper.AddVote(interaction.User.ID, id, payload.Base!.Value, payload.Reading!.Value, payload.Tracking!.Value, payload.Perception!.Value, purifier);

        map.RecalculateRating();
        MapHelper.Update(map);

        await interaction.Reply(HttpStatusCode.OK, map.Rating);
    }

    public class Payload
    {
        [JsonProperty("base")]
        public float? Base { get; set; }

        [JsonProperty("read")]
        public float? Reading { get; set; }

        [JsonProperty("track")]
        public float? Tracking { get; set; }

        [JsonProperty("percept")]
        public float? Perception { get; set; }
    }
}
