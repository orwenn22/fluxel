namespace fluxel.Modules.Messages;

public class UserOnlineStateMessage
{
    public long UserID { get; }
    public bool Online { get; }

    public UserOnlineStateMessage(long user, bool online)
    {
        UserID = user;
        Online = online;
    }
}
