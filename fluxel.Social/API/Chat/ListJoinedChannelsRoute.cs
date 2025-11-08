using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.Social.API.Chat;

public class ListJoinedChannelsRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/chat/channels/joined";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
        => await interaction.Reply(HttpStatusCode.OK, ChatHelper.WithMember(interaction.UserID).Select(ChatExtensions.ToAPI));
}
