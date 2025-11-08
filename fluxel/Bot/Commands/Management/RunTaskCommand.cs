using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using DSharpPlus.Entities;
using fluxel.Bot.Components;
using fluxel.Bot.Utils;
using fluxel.Tasks;
using fluXis.Utils;

namespace fluxel.Bot.Commands.Management;

public class RunTaskCommand : ISlashCommand
{
    public string Name => "run-task";
    public string Description => "Run a task";

    public IEnumerable<ISlashCommand.Option> Options => new List<ISlashCommand.Option>
    {
        new(OptionType.String, "name", "The name of the task.", true),
        new(OptionType.String, "args", "Constructor arguments, seperated by a colon. (:)", true),
    };

    public async void Handle(DiscordInteraction interaction)
    {
        await interaction.AcknowledgeEphemeral();

        var name = interaction.GetString("name")!;
        var strArgs = interaction.GetString("args")!;
        var argSplit = strArgs.Split(":");

        var tasks = Assembly.GetExecutingAssembly()
                            .GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IBasicTask)));

        var task = tasks.FirstOrDefault(t => t.Name.EqualsLower(name));

        if (task == null)
        {
            interaction.Reply("No task with this name found.", true);
            return;
        }

        var constructor = task.GetConstructors()[0];
        var parameters = new List<object>();

        foreach (var param in constructor.GetParameters())
        {
            switch (param.ParameterType.Name)
            {
                case "Int64":
                    parameters.Add(long.Parse(argSplit[param.Position], CultureInfo.InvariantCulture));
                    break;
            }
        }

        Activator.CreateInstance(task, constructor, parameters.ToArray());
    }
}
