using System.Collections.Generic;
using fluxel.Bot.Commands.Management.Groups;
using fluxel.Bot.Components;

namespace fluxel.Bot.Commands.Management;

public class GroupsCommandGroup : ISlashCommandGroup
{
    public string Name => "groups";
    public string Description => "Commands for managing groups.";

    public IEnumerable<ISlashCommand> Subcommands => new List<ISlashCommand>
    {
        new GroupsAddCommand()
    };
}
