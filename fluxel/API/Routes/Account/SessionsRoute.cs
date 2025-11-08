using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Helpers;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Account;

// TODO: move to /user/id/sessions
public class SessionsRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/account/sessions";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var sessions = SessionHelper.GetSessions(interaction.UserID);
        await interaction.Reply(HttpStatusCode.OK, sessions);
    }
}
