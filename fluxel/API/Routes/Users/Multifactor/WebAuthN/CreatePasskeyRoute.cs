using System;
using System.Net.Http;
using System.Threading.Tasks;
using Fido2NetLib;
using fluxel.API.Components;
using fluxel.Authentication;
using JetBrains.Annotations;
using Midori.API.Components.Interfaces;
using Midori.Networking;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace fluxel.API.Routes.Users.Multifactor.WebAuthN;

public class CreatePasskeyRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/@me/mfa/webauthn/create";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        bool valid = interaction.TryParseBody<RequestPayload>(out var payload) && !string.IsNullOrEmpty(payload.Credential);

        if (valid && payload != null)
            await registerCredential(interaction, payload.Token, payload.Credential);
        else
            await getOptions(interaction);
    }

    private async Task getOptions(FluxelAPIInteraction interaction)
    {
        var (guid, cred) = PasskeyAuth.CreateConfig(interaction.User);
        var json = JsonSerializer.Serialize(cred); // because OF COURSE it uses System.Text.Json
        await interaction.Reply(HttpStatusCode.OK, new RequestPayload
        {
            Credential = json,
            Token = guid
        });
    }

    private async Task registerCredential(FluxelAPIInteraction interaction, Guid guid, string credential)
    {
        var payload = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(credential);

        if (payload == null)
        {
            await interaction.Reply(HttpStatusCode.BadRequest, new { error = "Invalid credential." });
            return;
        }

        PasskeyAuth.CreateCredential(payload, guid, interaction.User);
        await interaction.ReplyMessage(HttpStatusCode.OK, "Credential created successfully.");
    }

    [UsedImplicitly]
    private class RequestPayload
    {
        /// <summary>
        /// JSON formatted credential creation options.
        /// </summary>
        [JsonProperty("credential")]
        public string Credential { get; set; } = null!;

        [JsonProperty("token")]
        public Guid Token { get; init; }
    }
}
