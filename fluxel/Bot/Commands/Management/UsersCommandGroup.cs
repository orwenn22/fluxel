using System.Collections.Generic;
using fluxel.Bot.Commands.Management.Users;
using fluxel.Bot.Components;

namespace fluxel.Bot.Commands.Management;

public class UsersCommandGroup : ISlashCommandGroup
{
    public string Name => "users";
    public string Description => "Commands for managing users.";

    public IEnumerable<ISlashCommand> Subcommands => new List<ISlashCommand>
    {
        new UserAddFlagCommand(),
        new UserRemoveFlagCommand(),
        new UsersAddGroupCommand(),
        new UsersResetPasswordCommand(),
        new SendKoFiEmail()
    };
}
