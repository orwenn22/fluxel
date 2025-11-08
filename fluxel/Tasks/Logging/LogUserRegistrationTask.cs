using System;
using DSharpPlus.Entities;
using fluxel.Bot;
using fluxel.Database.Helpers;
using fluxel.Utils;

namespace fluxel.Tasks.Logging;

public class LogUserRegistrationTask : IBasicTask
{
    public string Name => $"LogUserRegistration({id})";

    private long id { get; }

    public LogUserRegistrationTask(long id)
    {
        this.id = id;
    }

    public void Run()
    {
        var user = UserHelper.Get(id) ?? throw new ArgumentException($"No user with id {id} was found!");

        DiscordBot.GetChannel(DiscordBot.ChannelType.Registrations)?.SendMessageAsync(new DiscordMessageBuilder
        {
            Embed = new DiscordEmbedBuilder
            {
                Author = user.ToEmbedAuthor(),
                Description = "Just registered!",
                Color = new DiscordColor("#55ff55")
            }.WithFooter($"ID: {user.ID}").Build()
        });
    }
}
