using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluXis.Online.API.Models.Chat;
using Midori.Networking;

namespace fluxel.Social.API.Chat.Users;

public class JoinChannelRoute : IFluxelAPIRoute
{
    public string RoutePath => "/chat/channels/:channel/users/:userid";
    public HttpMethod Method => HttpMethod.Put;

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
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "you can only join public channels");
            return;
        }

        if (userid != interaction.UserID)
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "you cannot add other people to channels");
            return;
        }

        var result = ChatHelper.AddToChannel(chan.Name, userid);
        await interaction.Reply(result ? HttpStatusCode.Created : HttpStatusCode.NotModified);
        interaction.GetSocket()?.Client.AddToChatChannel(chan.ToAPI());
    }
}
