using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using fluxel.Bot.Components;
using fluxel.Bot.Utils;
using fluXis.Online.API.Models.Other;

namespace fluxel.Bot.Commands.Messages;

public class MessageCommand : ISlashCommand
{
    public string Name => "message";
    public string Description => "Send a message to all users in the game.";
    public Permissions Permissions => Permissions.Administrator;

    public IEnumerable<ISlashCommand.Option> Options => new List<ISlashCommand.Option>
    {
        new(OptionType.String, "text", "The text to send.", true),
        new(OptionType.String, "sub-text", "The sub text to send. Only used when non-disruptive is false.", false),
        new(OptionType.Boolean, "non-disruptive", "Whether the message should be non-disruptive.", false),
        new(OptionType.String, "users", "The users to send the message to. Separated by a space.", false)
    };

    public void Handle(DiscordInteraction interaction)
    {
        var count = 0;
        var users = interaction.GetString("users")?.Split(" ");

        var message = new ServerMessage
        {
            Text = interaction.GetString("text") ?? "",
            SubText = interaction.GetString("sub-text") ?? "",
            Type = interaction.GetBool("non-disruptive") ?? false ? "small" : "normal"
        };

        /*Program.NotificationConnections.ForEach(x =>
        {
            if (users is not null && !users.Contains(x.UserID.ToString()))
                return;

            count++;
            x.Client.DisplayMessage(message);
        });*/

        interaction.Reply($"Sent message to {count} users.", true);
    }
}
