using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using Midori.Networking;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace fluxel.Social.API.Chat.Messages;

public class GetChannelMessagesRoute : IFluxelAPIRoute
{
    public string RoutePath => "/chat/channels/:channel/messages";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetStringParameter("channel", out var ch))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("channel", "string"));
            return;
        }

        var channel = ChatHelper.GetChannel(ch);

        if (channel is null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, "This channel does not exist.");
            return;
        }

        if (!channel.Users.Contains(interaction.UserID))
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, "Current user is not part of this channel.");
            return;
        }

        var messages = ChatHelper.FromChannel(ch)
                                 .OrderByDescending(x => x.CreatedAt)
                                 .ToList().Take(50);

        messages.ForEach(m => m.Cache = interaction.Cache);
        await interaction.Reply(HttpStatusCode.OK, messages.Select(x => x.ToAPI()));
    }
}
