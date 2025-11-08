using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using fluxel.Bot.Components;
using fluxel.Bot.Utils;
using fluxel.Constants.Achievements;
using fluxel.Modules.Messages;

namespace fluxel.Bot.Commands.Testing;

public class AchievementTestCommand : ISlashCommand
{
    public string Name => "achievement";
    public string Description => "Test the achievement splash";
    public Permissions Permissions => Permissions.Administrator;

    public IEnumerable<ISlashCommand.Option> Options => new List<ISlashCommand.Option>
    {
        new(OptionType.Integer, "user", "The user to show the achievement for", true),
        new(OptionType.String, "id", "The achievement id", true)
    };

    public void Handle(DiscordInteraction interaction)
    {
        var user = interaction.GetInt("user");
        var id = interaction.GetString("id");

        if (user == null || string.IsNullOrWhiteSpace(id))
        {
            interaction.Reply("Invalid parameters", true);
            return;
        }

        var achievement = AchievementList.Find(id);

        if (achievement is null)
        {
            interaction.Reply("No achievement found with that id", true);
            return;
        }

        if (!(ServerHost.Instance.OnlineStates?.IsOnline(user.Value) ?? false))
        {
            interaction.Reply("User is not online", true);
            return;
        }

        ServerHost.Instance.SendMessage(new UserAchievementMessage(user.Value, achievement));
        interaction.Reply("Showing achievement", true);
    }
}
