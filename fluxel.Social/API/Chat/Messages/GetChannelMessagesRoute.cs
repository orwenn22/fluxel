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
        if (!interaction.TryGetStringParameter("channel", out var channel))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("channel", "string"));
            return;
        }

        var messages = ChatHelper.FromChannel(channel)
                                 .OrderByDescending(x => x.CreatedAt)
                                 .ToList().Take(50);

        messages.ForEach(m => m.Cache = interaction.Cache);
        await interaction.Reply(HttpStatusCode.OK, messages.Select(x => x.ToAPI()));
    }
}
