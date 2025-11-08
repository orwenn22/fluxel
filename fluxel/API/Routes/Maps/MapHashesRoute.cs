using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Maps;

public class MapHashesRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/maps/hashes";
    public HttpMethod Method => HttpMethod.Post;

    public IEnumerable<(string, string)> Validate(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryParseBody<List<long>>(out var payload))
            yield return ("_form", ResponseStrings.InvalidBodyJson);

        if (payload != null) interaction.AddCache("payload", payload);
    }

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetCache<List<long>>("payload", out var payload))
            throw new CacheMissingException("payload");

        interaction.Cache.Maps.EnsureAll();

        await interaction.Reply(HttpStatusCode.OK, payload.Select(x =>
        {
            var map = interaction.Cache.Maps.Get(x);
            return map is null ? string.Empty : $"{map.SHA256Hash}|{map.EffectSHA256Hash}|{map.StoryboardSHA256Hash}";
        }));
    }
}
