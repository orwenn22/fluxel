using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using fluxel.Bot.Commands;
using fluxel.Bot.Commands.Management;
using fluxel.Bot.Commands.Messages;
using fluxel.Bot.Commands.Testing;
using fluxel.Bot.Components;
using fluxel.Bot.Utils;
using fluxel.Config;
using fluxel.Database.Helpers;
using fluxel.Modules.Messages.Chat;
using Midori.Logging;

namespace fluxel.Bot;

public static class DiscordBot
{
    private static ServerConfig.DiscordConfig config = null!;

    public static DiscordClient? Bot { get; private set; }
    private static List<ISlashCommand>? commands { get; set; }

    public static async Task StartAsync(ServerConfig.DiscordConfig config)
    {
        if (Bot != null)
            throw new Exception("Bot is already running!");

        DiscordBot.config = config;

        if (string.IsNullOrWhiteSpace(config.Token))
            return;

        Bot = new DiscordClient(new DiscordConfiguration
        {
            Token = config.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
            AutoReconnect = true,
            MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.None
        });

        commands = new List<ISlashCommand>
        {
            new AchievementTestCommand(),
            new GroupsCommandGroup(),
            new ImageMessageCommand(),
            new MaintenanceCommand(),
            new MessageCommand(),
            new MapSetCommand(),
            new ModdingInactiveCommand(),
            new RecalculateCommand(),
            new RegenerateCommandGroup(),
            new RunTaskCommand(),
            new UserCommand(),
            new UsersCommandGroup(),
        };

        Bot.Ready += ready;
        Bot.InteractionCreated += onInteraction;
        Bot.MessageCreated += messageCreated;
        Bot.MessageDeleted += messageDeleted;
        await Bot.ConnectAsync();
    }

    public static async Task Stop()
    {
        if (Bot is null) return;

        Logger.Log("Shutting down Discord bot.");
        await Bot.DisconnectAsync();
    }

    private static async Task onInteraction(DiscordClient sender, InteractionCreateEventArgs args)
    {
        var command = commands!.FirstOrDefault(x => x.Name == args.Interaction.Data.Name);

        if (command == null)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Command not found!").AsEphemeral());
            return;
        }

        try
        {
            command.Handle(args.Interaction);
        }
        catch (Exception e)
        {
            args.Interaction.Reply("An error occurred while executing the command!", true);
            Logger.Error(e, "An error occurred while executing a command!");
        }
    }

    private static async Task ready(DiscordClient sender, ReadyEventArgs args)
    {
        Logger.Log($"Logged in as {Bot!.CurrentUser.Username}#{Bot.CurrentUser.Discriminator}!");

        if (commands == null) throw new Exception("Commands are null!");

        await Bot.BulkOverwriteGlobalApplicationCommandsAsync(commands.Select(x => x.Build()));
        await Bot.UpdateStatusAsync(new DiscordActivity("fluXis", ActivityType.Playing));
    }

    private static Task messageCreated(DiscordClient _, MessageCreateEventArgs args)
    {
        if (args.Channel.Id != config.ChatLink)
            return Task.CompletedTask;
        if (args.Author.IsBot)
            return Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(args.Message.Content))
        {
            Logger.Log("No message content. Is the message content intent enabled?");
            return Task.CompletedTask;
        }

        var user = UserHelper.GetByDiscordID(args.Author.Id);

        if (user is null)
        {
            try
            {
                args.Message.DeleteAsync();

                var member = args.Guild.GetMemberAsync(args.Author.Id).Result;
                var channel = member.CreateDmChannelAsync().Result;

                var sb = new StringBuilder();
                sb.AppendLine("Your message has not been sent because your discord account is not linked to any fluXis account.");
                sb.AppendLine("Head to https://auth.flux.moe/link/discord to link your account."); // should probably be set through envvars but i cba
                channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(sb.ToString()));
            }
            catch { }

            return Task.CompletedTask;
        }

        var message = ChatHelper.Add(user.ID, args.Message.Content, "general", args.Message.Id);
        ServerHost.Instance.SendMessage(new ChatMessageCreateMessage(message.ID));

        return Task.CompletedTask;
    }

    private static Task messageDeleted(DiscordClient _, MessageDeleteEventArgs args)
    {
        if (args.Channel.Id != config.ChatLink)
            return Task.CompletedTask;

        var message = ChatHelper.GetByDiscordID(args.Message.Id);
        if (message is null) return Task.CompletedTask;

        ChatHelper.Delete(message);
        ServerHost.Instance.SendMessage(new ChatMessageDeleteMessage(message.ID));
        return Task.CompletedTask;
    }

    private static DiscordChannel? getChannel(ulong id) => Bot?.GetChannelAsync(id).Result;

    public static DiscordChannel? GetChannel(ChannelType type) => type switch
    {
        ChannelType.Registrations => getChannel(config.Registrations),
        ChannelType.Logging => getChannel(config.Logging),
        ChannelType.MapSubmissions => getChannel(config.MapSubmissions),
        ChannelType.MapRanked => getChannel(config.MapRanked),
        ChannelType.Queue => getChannel(config.QueueUpdates),
        ChannelType.MapFirstPlace => getChannel(config.MapFirstPlace),
        ChannelType.ChatLink => getChannel(config.ChatLink),
        _ => null
    };

    public static void SendException(Exception e)
    {
        var channel = GetChannel(ChannelType.Logging);

        var stackTrace = e.StackTrace?[..Math.Min(e.StackTrace.Length, 1000)];

        var message = new DiscordMessageBuilder()
                      .WithContent("<@386436194709274627>")
                      .WithAllowedMention(new UserMention(386436194709274627))
                      .WithEmbed(new DiscordEmbedBuilder()
                                 .WithTitle("Unhandled exception")
                                 .WithDescription(e.Message)
                                 .AddField("Stack trace", $"```{stackTrace ?? "No stack trace"}```")
                                 .WithColor(DiscordColor.Red));

        channel?.SendMessageAsync(message);
    }

    public enum ChannelType
    {
        Logging,
        Registrations,
        MapSubmissions,
        MapRanked,
        Queue,
        MapFirstPlace,
        ChatLink
    }
}
