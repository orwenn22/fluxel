using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes;

public class EventsRoute : IFluxelAPIRoute
{
    public string RoutePath => "/events";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var events = EventHelper.GetActive();
        await interaction.Reply(HttpStatusCode.OK, events);
    }
}
