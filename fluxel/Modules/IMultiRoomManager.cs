using fluXis.Online.API.Models.Multi;

namespace fluxel.Modules;

public interface IMultiRoomManager
{
    MultiplayerRoom? WithPlayer(long id);
}
