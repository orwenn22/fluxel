using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Modules.Messages.Chat;
using fluXis.Online.API.Models.Chat;
using fluXis.Online.API.Payloads.Chat;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.Social.API.Chat.Messages;

public class SendChatMessageRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/chat/channels/:channel/messages";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetStringParameter("channel", out var chan))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("channel", "string"));
            return;
        }

        if (!interaction.TryParseBody<ChatMessagePayload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        var channel = ChatHelper.GetChannel(chan);

        if (channel is null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, "The specified channel does not exist.");
            return;
        }

        if (!channel.Users.Contains(interaction.UserID))
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "Current user is not part of this channel.");
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

        var message = ChatHelper.Add(interaction.UserID, payload.Content, channel.Name);

        if (channel.Type == APIChannelType.Private)
        {
            if (ChatHelper.AddToChannel(channel.Name, channel.Target1!.Value))
                NotificationsModule.SocketByID(channel.Target1.Value)?.Client.AddToChatChannel(channel.ToAPI());
            if (ChatHelper.AddToChannel(channel.Name, channel.Target2!.Value))
                NotificationsModule.SocketByID(channel.Target2.Value)?.Client.AddToChatChannel(channel.ToAPI());
        }

        ServerHost.Instance.SendMessage(new ChatMessageCreateMessage(message.ID));
        await interaction.Reply(HttpStatusCode.Created, message);
    }
}
