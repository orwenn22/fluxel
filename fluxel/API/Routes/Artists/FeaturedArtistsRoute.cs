using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes.Artists;

public class FeaturedArtistsRoute : IFluxelAPIRoute
{
    public string RoutePath => "/artists";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
        => await interaction.Reply(HttpStatusCode.OK, FeaturedArtistHelper.AllArtists);
}
