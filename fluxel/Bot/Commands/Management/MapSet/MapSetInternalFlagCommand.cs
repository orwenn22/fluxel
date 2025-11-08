using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using fluxel.Bot.Components;
using fluxel.Bot.Utils;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Maps;
using osu.Framework.Extensions;

namespace fluxel.Bot.Commands.Management.MapSet;

public class MapSetInternalFlagCommand : ISlashCommand
{
    public string Name => "int-flags";
    public string Description => "internal mapset flags";

    public IEnumerable<ISlashCommand.Option> Options => new[]
    {
        new(OptionType.Integer, "id", "id of the set", true),
        new ISlashCommand.Option(OptionType.String, "flag", "the flag to change", true).WithChoices(Enum.GetValues<InternalSetFlags>().Select(f =>
        {
            var desc = f.GetDescription();
            return new ISlashCommand.Choice(desc, f.ToString());
        }).ToArray()),
        new(OptionType.Boolean, "enabled", "read the name", true)
    };

    public void Handle(DiscordInteraction interaction)
    {
        var id = interaction.GetInt("id")!.Value;
        var flagStr = interaction.GetString("flag")!;
        var enabled = interaction.GetBool("enabled")!.Value;

        if (!Enum.TryParse<InternalSetFlags>(flagStr, out var flag))
        {
            interaction.Reply($"'{flagStr}' is not valid.", true);
            return;
        }

        var set = MapSetHelper.Get(id);

        if (set is null)
        {
            interaction.Reply("set not found", true);
            return;
        }

        var cur = set.InternalFlags;
        var result = cur;

        if (enabled)
            result |= flag;
        else
            result &= ~flag;

        if (result == cur)
        {
            interaction.Reply("nothing changed", true);
            return;
        }

        set.InternalFlags = result;
        MapSetHelper.Update(set);

        var sb = new StringBuilder();
        sb.AppendLine($"**{set.Title} - {set.Artist}**");
        sb.AppendLine($"uploaded by **{set.GetCreator()?.Username}**");
        sb.AppendLine();
        sb.AppendLine($"**{flag} -> {(enabled ? "enabled" : "disabled")}**");

        interaction.Reply(sb.ToString(), true);
    }
}
