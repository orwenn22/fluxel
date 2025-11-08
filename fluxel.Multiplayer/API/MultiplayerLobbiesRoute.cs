using fluxel.API.Components;
using fluxel.Multiplayer.Lobby;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.Multiplayer.API;

public class MultiplayerLobbiesRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/multi/lobbies";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
        => await interaction.Reply(HttpStatusCode.OK, MultiplayerRoomManager.Lobbies.Select(x => x.ToAPI()));
}
