using System.Collections.Generic;
using DSharpPlus.Entities;
using fluxel.Bot.Components;
using fluxel.Bot.Utils;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluxel.Models.Maps;
using fluXis.Online.API.Models.Maps.Modding;

namespace fluxel.Bot.Commands;

public class ModdingInactiveCommand : ISlashCommand
{
    public string Name => "modding-inactive";
    public string Description => "Marks a mapset as inactive and removes if from the modding queue.";

    public IEnumerable<ISlashCommand.Option> Options => new List<ISlashCommand.Option>
    {
        new(OptionType.Integer, "id", "The ID of the mapset to mark as inactive.", true)
    };

    public void Handle(DiscordInteraction interaction)
    {
        var set = MapSetHelper.Get(interaction.GetInt("id")!.Value);

        if (set is null)
        {
            interaction.Reply(ResponseStrings.MapSetNotFound, true);
            return;
        }

        if (set.Status != MapStatus.Pending)
        {
            interaction.Reply("Not in queue?", true);
            return;
        }

        if (!set.AddModdingEntry(APIModdingActionType.Deny, 0, out var error))
        {
            interaction.Reply(error, true);
            return;
        }

        MapSetHelper.CreateModAction(set.ID, 0, APIModdingActionType.Deny, "This mapset has been marked as inactive as it has not been updated in 2 weeks.");
        interaction.Reply("okayge :+1::+1:", true);
    }
}
