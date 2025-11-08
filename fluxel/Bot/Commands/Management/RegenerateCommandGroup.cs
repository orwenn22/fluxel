using System.Collections.Generic;
using fluxel.Bot.Commands.Management.Regenerate;
using fluxel.Bot.Components;

namespace fluxel.Bot.Commands.Management;

public class RegenerateCommandGroup : ISlashCommandGroup
{
    public string Name => "regenerate";
    public string Description => "Regenerate different things.";

    public IEnumerable<ISlashCommand> Subcommands => new List<ISlashCommand>
    {
        new RegenerateMapSetAssetsCommand()
    };
}
