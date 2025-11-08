using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluXis.Online.API.Payloads.Users;
using Midori.API.Components.Interfaces;
using Midori.Networking;
using Midori.Utils;
using Newtonsoft.Json.Linq;
using osu.Framework.IO.Network;

namespace fluxel.API.Routes.Users.Connections;

public class UserConnectionsRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/user/:id/connections";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!interaction.TryParseBody<UserConnectionCreatePayload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        if (!UserHelper.TryGet(id, out var user))
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.UserNotFound);
            return;
        }

        if (user.ID != interaction.UserID)
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "You can't do this on another user.");
            return;
        }

        switch (payload.Provider)
        {
            case "discord":
                await discord(interaction, payload);
                break;

            case "steam":
                await steam(interaction, payload);
                break;

            case "kofi":
                await kofi(interaction, payload);
                break;

            default:
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, $"Unknown provider '{payload.Provider}'.");
                break;
        }
    }

    private static async Task kofi(FluxelAPIInteraction interaction, UserConnectionCreatePayload payload)
    {
        if (string.IsNullOrEmpty(payload.Token))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.MissingJsonField<UserConnectionCreatePayload>(nameof(payload.Token)));
            return;
        }

        if (!string.IsNullOrWhiteSpace(interaction.User.KoFiEmail))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "You have already linked a Ko-Fi email.");
            return;
        }

        if (!Donations.Connect(payload.Token, interaction.UserID, out var error))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, error);
            return;
        }

        await interaction.Reply(HttpStatusCode.OK);
        Donations.Update(interaction.UserID);
    }

    private static async Task steam(FluxelAPIInteraction interaction, UserConnectionCreatePayload payload)
    {
        if (string.IsNullOrEmpty(payload.Token))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.MissingJsonField<UserConnectionCreatePayload>(nameof(payload.Token)));
            return;
        }

        var req = new WebRequest("https://partner.steam-api.com/ISteamUserAuth/AuthenticateUserTicket/v1/");
        req.AddParameter("key", ServerHost.Configuration.Steam.WebKey);
        req.AddParameter("appid", ServerHost.Configuration.Steam.AppID.ToString());
        req.AddParameter("ticket", payload.Token);
        // req.AddParameter("identity", "");

        await req.PerformAsync();

        var result = await new StreamReader(req.ResponseStream).ReadToEndAsync();

        var json = result.Deserialize<JObject>()!;
        var response = json["response"]?.ToObject<JObject>() ?? throw new InvalidOperationException();
        var param = response["params"]?.ToObject<JObject>() ?? throw new InvalidOperationException();
        var id = param["steamid"]?.ToObject<ulong>() ?? throw new InvalidOperationException();

        UserHelper.UpdateLocked(interaction.UserID, u => u.SteamID = id);
        await interaction.Reply(HttpStatusCode.OK, id);
    }

    private static async Task discord(FluxelAPIInteraction interaction, UserConnectionCreatePayload payload)
    {
        if (string.IsNullOrEmpty(payload.Token))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.MissingJsonField<UserConnectionCreatePayload>(nameof(payload.Token)));
            return;
        }

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {payload.Token}");
        var res = await client.GetAsync("https://discord.com/api/users/@me");

        if (!res.IsSuccessStatusCode)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, $"Failed to communicate with Discord.");
            return;
        }

        var json = await res.Content.ReadAsStringAsync();
        var data = json.Deserialize<JObject>()!;

        if (!data.ContainsKey("id"))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, $"Failed fetch user.");
            return;
        }

        ulong id = ulong.Parse(data["id"]!.Value<string>()!);
        UserHelper.UpdateLocked(interaction.UserID, u => u.DiscordID = id);
        await interaction.Reply(HttpStatusCode.OK);
    }
}
