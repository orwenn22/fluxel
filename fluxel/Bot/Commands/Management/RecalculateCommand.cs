using System.Collections.Generic;
using DSharpPlus.Entities;
using fluxel.Bot.Components;
using fluxel.Bot.Utils;
using fluxel.Database.Helpers;
using fluxel.Tasks.Clubs;
using fluxel.Tasks.Maps;
using fluxel.Tasks.MapSets;
using fluxel.Tasks.Scores;
using fluxel.Tasks.Users;

namespace fluxel.Bot.Commands.Management;

public class RecalculateCommand : ISlashCommand
{
    public string Name => "recalculate";
    public string Description => "Recalculate different things.";

    public IEnumerable<ISlashCommand.Option> Options => new List<ISlashCommand.Option>
    {
        new ISlashCommand.Option(OptionType.String, "type", "The type of recalculation to perform.", true).WithChoices(
            new ISlashCommand.Choice("Clubs", "clubs"),
            new ISlashCommand.Choice("Club Claims", "club-claims"),
            new ISlashCommand.Choice("Club Scores", "club-scores"),
            new ISlashCommand.Choice("Maps", "maps"),
            new ISlashCommand.Choice("Map filenames", "map-filenames"),
            new ISlashCommand.Choice("Previews", "previews"),
            new ISlashCommand.Choice("Scores", "scores"),
            new ISlashCommand.Choice("Users", "users")
        )
    };

    public async void Handle(DiscordInteraction interaction)
    {
        await interaction.AcknowledgeEphemeral();

        var type = interaction.GetString("type");

        switch (type)
        {
            case "clubs":
                ClubHelper.All.ForEach(c => ServerHost.Instance.Scheduler.Schedule(new RecalculateClubTask(c.ID)));
                break;

            case "club-claims":
                ServerHost.Instance.Scheduler.Schedule(new RefreshClubClaimsBulkTask());
                break;

            case "club-scores":
            {
                var maps = MapHelper.All;
                ClubHelper.All.ForEach(c => maps.ForEach(m => ServerHost.Instance.Scheduler.Schedule(new RecalculateClubScoreTask(m.ID, c.ID))));
                break;
            }

            case "maps":
                MapHelper.All.ForEach(m => ServerHost.Instance.Scheduler.Schedule(new RecalculateMapTask(m.ID)));
                break;

            case "previews":
                ServerHost.Instance.Scheduler.Schedule(new RegeneratePreviewsBulkTask());
                break;

            case "scores":
                ScoreHelper.All.ForEach(s => ServerHost.Instance.Scheduler.Schedule(new RecalculateScoreTask(s.ID)));
                break;

            case "users":
                UserHelper.All.ForEach(u => ServerHost.Instance.Scheduler.Schedule(new RecalculateUserTask(u.ID)));
                break;

            default:
                interaction.Reply("Invalid recalculation type.", true);
                break;
        }

        interaction.Followup("Created tasks!");
    }
}
