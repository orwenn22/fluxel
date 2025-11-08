using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluXis.Online.API.Payloads.Collections;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Collections;

public class CreateCollectionRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/collections";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryParseBody<CollectionCreatePayload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }
    }
}

