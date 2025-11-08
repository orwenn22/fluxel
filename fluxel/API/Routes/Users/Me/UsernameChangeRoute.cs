using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluxel.Utils;
using Midori.API.Components.Interfaces;
using Midori.Networking;
using Newtonsoft.Json;

namespace fluxel.API.Routes.Users.Me;

public class UsernameChangeRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/users/@me/username";
    public HttpMethod Method => HttpMethod.Patch;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.User.ForceNameChange)
        {
            await interaction.ReplyMessage(HttpStatusCode.PaymentRequired, "No name changes available.");
            return;
        }

        if (!interaction.TryParseBody<Payload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        if (string.IsNullOrWhiteSpace(payload.Username) || payload.Username.Length is < 3 or > 16)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Username must be between 3 and 16 characters!");
            return;
        }

        if (!payload.Username.Validate(StringValidator.ValidationType.Username))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Username can only contain A-Z, a-z, 0-9 and _!");
            return;
        }

        if (payload.Username.IsBlacklisted())
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "This username is not available!");
            return;
        }

        UserHelper.UpdateLocked(interaction.UserID, u =>
        {
            u.Username = payload.Username;
            u.ForceNameChange = false;
        });

        await interaction.Reply(HttpStatusCode.OK);
    }

    private class Payload
    {
        [JsonProperty("username")]
        public string? Username { get; set; }
    }
}
