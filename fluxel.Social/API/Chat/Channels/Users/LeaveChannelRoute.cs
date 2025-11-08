using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluXis.Online.API.Models.Chat;
using Midori.Networking;

namespace fluxel.Social.API.Chat.Channels.Users;

public class LeaveChannelRoute : IFluxelAPIRoute
{
    public string RoutePath => "/chat/channels/:channel/users/:userid";
    public HttpMethod Method => HttpMethod.Delete;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetStringParameter("channel", out var channel))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("channel", "string"));
            return;
        }

        if (!interaction.TryGetLongParameter("userid", out var userid))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("userid", "long"));
            return;
        }

        var chan = ChatHelper.GetChannel(channel);

        if (chan == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, "channel not found");
            return;
        }

        if (chan.Type != APIChannelType.Public)
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "you can only leave public channels");
            return;
        }

        if (userid != interaction.UserID)
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "you cannot remove other people from channels");
            return;
        }

        var result = ChatHelper.RemoveFromChannel(chan.Name, userid);
        await interaction.Reply(result ? HttpStatusCode.OK : HttpStatusCode.NotModified);

        NotificationsModule.Sockets.FirstOrDefault(x => x.UserID == userid)?.Client.RemoveFromChatChannel(channel);
    }
}
