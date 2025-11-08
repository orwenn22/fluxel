using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using fluxel.Bot.Components;
using fluxel.Bot.Utils;
using fluXis.Online.API.Models.Other;

namespace fluxel.Bot.Commands.Messages;

public class ImageMessageCommand : ISlashCommand
{
    public string Name => "image-message";
    public string Description => "Send a message to all users in the game.";
    public Permissions Permissions => Permissions.Administrator;

    public IEnumerable<ISlashCommand.Option> Options => new List<ISlashCommand.Option>
    {
        new(OptionType.String, "text", "The text to send.", true),
        new(OptionType.String, "path", "The URL to the image to send.", true),
        new(OptionType.String, "users", "The users to send the message to. Separated by a space.", false)
    };

    public void Handle(DiscordInteraction interaction)
    {
        var count = 0;
        var users = interaction.GetString("users")?.Split(" ");

        var message = new ServerMessage
        {
            Text = interaction.GetString("text") ?? "",
            Type = "image",
            Path = interaction.GetString("path") ?? ""
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
