using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.Social.API.Chat;

public class ListChatChannelsRoute : IFluxelAPIRoute
{
    public string RoutePath => "/chat/channels";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
        => await interaction.Reply(HttpStatusCode.OK, ChatHelper.PublicChannels.Select(ChatExtensions.ToAPI));
}
