using fluxel.API.Components;
using fluxel.Models.Users;

namespace fluxel.Social;

public static class NotificationExtensions
{
    public static NotificationSocket? GetSocket(this User user)
        => NotificationsModule.SocketByID(user.ID);

    public static NotificationSocket? GetSocket(this FluxelAPIInteraction interaction)
        => NotificationsModule.SocketByID(interaction.UserID);
}
