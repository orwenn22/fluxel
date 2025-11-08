using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Users;

public class MeRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/users/@me";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
        => await interaction.Reply(HttpStatusCode.OK, interaction.User.ToAPI());
}
