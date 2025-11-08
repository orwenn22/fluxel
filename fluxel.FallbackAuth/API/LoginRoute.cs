using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluXis.Online.API.Payloads.Auth;
using fluXis.Online.API.Responses.Auth;
using Midori.Networking;

namespace fluxel.FallbackAuth.API;

public class LoginRoute : IFluxelAPIRoute
{
    public string RoutePath => "/auth/login";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryParseBody<LoginPayload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        if (string.IsNullOrWhiteSpace(payload.Username) || string.IsNullOrWhiteSpace(payload.Password))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Username and password must not be empty.");
            return;
        }

        if (!UserHelper.TryGet(payload.Username, out var user))
        {
            await interaction.ReplyMessage(HttpStatusCode.Unauthorized, "No user with that username");
            return;
        }

        var ua = interaction.Request.Headers["User-Agent"] ?? "";

        var session = SessionHelper.GetSimilar(user.ID, interaction.RemoteIP.ToString())
                      ?? SessionHelper.Create(user.ID, interaction.RemoteIP.ToString(), ua).Result;

        await interaction.Reply(HttpStatusCode.OK, new LoginResponse(session.Token, session.UserID));
    }
}
