using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluxel.Tasks.Logging;
using fluxel.Tasks.Users;
using fluxel.Utils;
using fluXis.Online.API.Payloads.Auth;
using fluXis.Online.API.Responses.Auth;
using Midori.Networking;

namespace fluxel.FallbackAuth.API;

public class RegisterRoute : IFluxelAPIRoute
{
    public string RoutePath => "/auth/register";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var ua = interaction.Request.Headers["User-Agent"] ?? string.Empty;

        if (!interaction.TryParseBody<RegisterPayload>(out var json))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        if (string.IsNullOrWhiteSpace(json.Username) || string.IsNullOrWhiteSpace(json.Password) || string.IsNullOrWhiteSpace(json.Email))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Username, password and email must not be empty.");
            return;
        }

        if (json.Username.Length is < 3 or > 16)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Username must be between 3 and 16 characters!");
            return;
        }

        if (!json.Username.Validate(StringValidator.ValidationType.Username))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Username can only contain A-Z, a-z, 0-9 and _!");
            return;
        }

        if (json.Username.UsernameExists())
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Username is already taken!");
            return;
        }

        var ip = interaction.RemoteIP.ToString();
        var country = await IpUtils.GetCountryCode(ip);
        var user = UserHelper.Add(json.Username, json.Email, json.Password, country);

        var session = SessionHelper.Create(user.ID, ip, "fluXis").Result;
        await interaction.Reply(HttpStatusCode.OK, new RegisterResponse(session.Token));

        ServerHost.Instance.Scheduler.Schedule(new LogUserRegistrationTask(user.ID));
        ServerHost.Instance.Scheduler.Schedule(new AddToDefaultChannelsTask(user.ID));
    }
}
