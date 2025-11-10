using fluxel.Modules;
using fluxel.Modules.Messages;
using fluxel.Multiplayer.Lobby;
using fluXis.Online.API.Models.Multi;
using Midori.Networking;

namespace fluxel.Multiplayer;

public class MultiplayerModule : IModule, IMultiRoomManager
{
    public static HttpConnectionManager<MultiplayerSocket> Sockets { get; private set; } = null!;

    public void OnLoad(ServerHost host)
    {
        Sockets = host.Server.MapModule<MultiplayerSocket>("/multiplayer");

        MultiplayerRoomManager.StartThread();
    }

    public void OnMessage(object data)
    {
        switch (data)
        {
            case UserOnlineStateMessage onl:
            {
                var room = MultiplayerRoomManager.GetCurrentRoom(onl.UserID);
                room?.Disconnect(onl.UserID);
                break;
            }
        }
    }

    MultiplayerRoom? IMultiRoomManager.WithPlayer(long id) => MultiplayerRoomManager.GetCurrentRoom(id)?.ToAPI();
}
