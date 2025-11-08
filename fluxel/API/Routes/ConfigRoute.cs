using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluXis.Online.API.Models;
using Midori.Networking;

namespace fluxel.API.Routes;

public class ConfigRoute : IFluxelAPIRoute
{
    public string RoutePath => "/config";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction) => await interaction.Reply(HttpStatusCode.OK, new APIConfig
    {
        AssetsUrl = ServerHost.Configuration.Urls.Assets,
        WebsiteUrl = ServerHost.Configuration.Urls.Website,
        WikiUrl = ServerHost.Configuration.Urls.Website + "/wiki"
    });
}
