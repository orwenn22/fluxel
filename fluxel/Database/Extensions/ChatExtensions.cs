using fluxel.Models.Chat;
using fluXis.Online.API.Models.Chat;

namespace fluxel.Database.Extensions;

public static class ChatExtensions
{
    public static APIChatChannel ToAPI(this ChatChannel channel) => new()
    {
        Name = channel.Name,
        Type = channel.Type,
        UserCount = channel.Users.Count
    };
}
