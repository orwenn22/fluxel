using fluxel.Modules;
using fluxel.Multiplayer.Lobby;
using Midori.Networking;

namespace fluxel.Multiplayer;

public class MultiplayerModule : IModule
{
    public static HttpConnectionManager<MultiplayerSocket> Sockets { get; private set; } = null!;

    public void OnLoad(ServerHost host)
    {
        Sockets = host.Server.MapModule<MultiplayerSocket>("/multiplayer");

        MultiplayerRoomManager.StartThread();
    }
}
