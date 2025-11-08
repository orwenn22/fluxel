using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.API.Components.Interfaces;
using Midori.Networking;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace fluxel.Social.API.Chat.Channels;

public class DeleteChatMessageRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/chat/channels/:channel/messages/:message";
    public HttpMethod Method => HttpMethod.Delete;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetStringParameter("channel", out var channel))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("channel", "string"));
            return;
        }

        if (!interaction.TryGetStringParameter("message", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("message", "string"));
            return;
        }

        if (!interaction.User.IsModerator())
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, ResponseStrings.NoPermission);
            return;
        }

        var message = ChatHelper.Get(channel, id);

        if (message is null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, "Message not found.");
            return;
        }

        ChatHelper.Delete(message);
        NotificationsModule.Sockets.ForEach(c => c.Client.DeleteChatMessage(channel, id));
        await interaction.ReplyMessage(HttpStatusCode.OK,"Deleted.");
    }
}
