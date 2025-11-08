using System.Collections.Generic;
using fluxel.Bot.Commands.Management.MapSet;
using fluxel.Bot.Components;

namespace fluxel.Bot.Commands.Management;

public class MapSetCommand : ISlashCommandGroup
{
    public string Name => "mapset";
    public string Description => "mapset related commands";

    public IEnumerable<ISlashCommand> Subcommands => new ISlashCommand[]
    {
        new MapSetInternalFlagCommand()
    };
}
