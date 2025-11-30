using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluxel.Modules.Messages.Chat;
using fluXis.Online.API.Payloads.Chat;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.Social.API.Chat.Channels;

public class SendChatMessageRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/chat/channels/:channel/messages";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetStringParameter("channel", out var channel))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("channel", "string"));
            return;
        }

        if (!interaction.TryParseBody<ChatMessagePayload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        if (string.IsNullOrEmpty(payload.Content))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Message cannot be empty.");
            return;
        }

        if (payload.Content.Length > 2048)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Message exceeds 2048 characters.");
            return;
        }

        var message = ChatHelper.Add(interaction.UserID, payload.Content, channel);
        ServerHost.Instance.SendMessage(new ChatMessageCreateMessage(message.ID));
        await interaction.Reply(HttpStatusCode.Created, message);
    }
}
