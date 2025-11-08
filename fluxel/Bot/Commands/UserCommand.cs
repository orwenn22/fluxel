using System.Collections.Generic;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using fluxel.Bot.Components;
using fluxel.Bot.Utils;
using fluxel.Database.Helpers;
using fluxel.Utils;

namespace fluxel.Bot.Commands;

public class UserCommand : ISlashCommand
{
    public string Name => "user";
    public string Description => "Get information about a user.";
    public Permissions Permissions => Permissions.SendMessages;

    public IEnumerable<ISlashCommand.Option> Options => new List<ISlashCommand.Option>
    {
        new(OptionType.String, "user", "The user to get information about.", true)
    };

    public void Handle(DiscordInteraction interaction)
    {
        var userstr = interaction.GetString("user")!;

        var user = int.TryParse(userstr, out var id) ? UserHelper.Get(id) : UserHelper.Get(userstr);

        if (user == null)
        {
            interaction.Reply("User not found.", true);
            return;
        }

        var group = user.Groups.FirstOrDefault();
        var color = group is null ? DiscordColor.Blurple : ColorUtils.ParseHex(group.Color);

        var embed = new DiscordEmbedBuilder
        {
            Author = user.ToEmbedAuthor(),
            Color = color,
            ImageUrl = user.BannerUrl,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = user.AvatarUrl }
        };

        if (!string.IsNullOrEmpty(user.DisplayName))
            embed.AddField("Display Name", user.DisplayName, true);

        if (user.Club != null)
            embed.AddField("Club", user.Club.Name, true);

        embed.AddField("Registered", $"<t:{user.CreatedAt}:R>", true);
        embed.AddField("Last Seen", user.IsOnline ? "Right Now" : $"<t:{user.LastLogin}:R>", true);
        embed.AddField("Global Rank", $"#{user.GetGlobalRank()}", true);
        embed.AddField("Country Rank", $"#{user.GetCountryRank()}", true);
        embed.AddField("Overall Rating", $"{user.OverallRating:0.00}", true);
        embed.AddField("Potential Rating", $"{user.PotentialRating:0.00}", true);
        embed.WithFooter($"ID: {user.ID}");

        interaction.ReplyEmbed(embed);
    }
}
