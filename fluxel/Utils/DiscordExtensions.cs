using DSharpPlus.Entities;
using fluxel.Models.Users;

namespace fluxel.Utils;

public static class DiscordExtensions
{
    public static DiscordEmbedBuilder.EmbedAuthor ToEmbedAuthor(this User user) => new()
    {
        Name = user.Username,
        IconUrl = user.AvatarUrl,
        Url = user.Url
    };
}
