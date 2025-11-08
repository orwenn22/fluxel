using fluXis.Online.API.Models.Users;

namespace fluxel.Modules;

public interface IOnlineStateManager
{
    long[] AllOnline { get; }

    bool IsOnline(long user);
    APIActivity? GetActivity(long user);
}
