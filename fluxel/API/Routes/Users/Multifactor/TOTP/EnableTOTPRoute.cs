using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Authentication;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluXis.Online.API.Payloads.Auth.Multifactor;
using fluXis.Online.API.Responses.Auth.Multifactor;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Users.Multifactor.TOTP;

public class EnableTOTPRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/user/:id/mfa/totp/enable";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!interaction.TryParseBody<TOTPEnablePayload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        if (interaction.UserID != id)
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "You can't do this on another user.");
            return;
        }

        if (string.IsNullOrEmpty(payload.Password))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Missing password.");
            return;
        }

        if (!PasswordAuth.VerifyPassword(payload.Password, interaction.User.Password))
        {
            await interaction.ReplyMessage(HttpStatusCode.Unauthorized, "The provided password is incorrect");
            return;
        }

        if (string.IsNullOrEmpty(payload.SharedKey) || string.IsNullOrEmpty(payload.Code))
        {
            await interaction.ReplyMessage(HttpStatusCode.OK, "Valid credentials, you can proceed to the next step.");
            return;
        }

        if (AuthHelper.HasCode(id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "TOTP is already enabled.");
            return;
        }

        if (payload.SharedKey.Length != 32)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Shared key is not 32 characters long.");
            return;
        }

        if (payload.Code.Length != 6)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Code is not 6 characters long.");
            return;
        }

        if (!TimedCodeAuth.Verify(payload.SharedKey, payload.Code))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Code can not be validated against shared key.");
            return;
        }

        var codes = AuthHelper.CreateTimeBased(id, payload.SharedKey);

        UserHelper.UpdateLocked(interaction.UserID, u => u.HasTOTP = true);
        await interaction.Reply(HttpStatusCode.Created, new TOTPEnableResponse(codes.Select(x => x.Code).ToList()));
    }
}
