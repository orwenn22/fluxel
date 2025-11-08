using System;
using System.Collections.Generic;
using fluxel.Models.Chat;
using fluXis.Online.API.Models.Chat;
using MongoDB.Driver;
using ChatMessage = fluxel.Models.Chat.ChatMessage;

namespace fluxel.Database.Helpers;

public static class ChatHelper
{
    #region Messages

    private static IMongoCollection<ChatMessage> messages => MongoDatabase.GetCollection<ChatMessage>("chat_messages");

    public static void Add(ChatMessage message) => messages.InsertOne(message);

    public static ChatMessage? Get(string channel, string id) => Guid.TryParse(id, out var g) ? Get(channel, g) : null;
    public static ChatMessage? Get(string channel, Guid id) => messages.Find(x => x.Channel == channel && x.ID == id).FirstOrDefault();

    public static void Delete(ChatMessage message)
    {
        message.Deleted = true;
        messages.ReplaceOne(x => x.ID == message.ID, message);
    }

    public static IEnumerable<ChatMessage> FromChannel(string channel) => messages.Find(x => x.Channel == channel && !x.Deleted).ToList();

    #endregion

    #region Channels

    private static IMongoCollection<ChatChannel> channels => MongoDatabase.GetCollection<ChatChannel>("chat-channels");

    public static IReadOnlyList<ChatChannel> PublicChannels => channels.Find(x => x.Type == APIChannelType.Public).ToList();

    public static void CreatePublicChannel(string name, List<long>? ids = default) => channels.InsertOne(new ChatChannel(name, APIChannelType.Public)
    {
        Users = ids ?? new List<long>()
    });

    public static ChatChannel? GetChannel(string name) => channels.Find(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant()).FirstOrDefault();

    public static IEnumerable<ChatChannel> WithMember(long id) => channels.Find(x => x.Users.Contains(id)).ToList();

    public static bool AddToChannel(string name, long id)
    {
        var chan = GetChannel(name);

        if (chan is null || chan.Users.Contains(id))
            return false;

        chan.Users.Add(id);
        update(chan);
        return true;
    }

    public static bool RemoveFromChannel(string name, long id)
    {
        var chan = GetChannel(name);

        if (chan is null || !chan.Users.Contains(id))
            return false;

        chan.Users.Remove(id);
        update(chan);
        return true;
    }

    private static void update(ChatChannel channel)
        => channels.ReplaceOne(x => x.Name == channel.Name, channel);

    #endregion
}
