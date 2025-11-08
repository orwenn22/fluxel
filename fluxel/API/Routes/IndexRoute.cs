using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using Midori.Networking;

namespace fluxel.API.Routes;

public class IndexRoute : IFluxelAPIRoute
{
    public string RoutePath => "/";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
        => await interaction.ReplyMessage(HttpStatusCode.OK, "Welcome to fluxel, the API for fluXis! You can see the API docs at https://fluxis.flux.moe/wiki/api");
}
