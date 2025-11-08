using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluxel.Models.Chat;
using fluXis.Online.API.Payloads.Chat;
using Midori.API.Components.Interfaces;
using Midori.Networking;
using osu.Framework.Extensions.IEnumerableExtensions;

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
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "message is empty");
            return;
        }

        var message = new ChatMessage
        {
            SenderID = interaction.UserID,
            Content = payload.Content,
            Channel = channel
        };

        ChatHelper.Add(message);
        NotificationsModule.Sockets.ForEach(c => c.Client.ReceiveChatMessage(message.ToAPI()));
        await interaction.Reply(HttpStatusCode.Created, message);
    }
}
