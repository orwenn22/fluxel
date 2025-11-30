using System;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluXis.Online.API.Models.Chat;
using fluXis.Online.API.Models.Users;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace fluxel.Models.Chat;

[JsonObject(MemberSerialization.OptIn)]
public class ChatMessage
{
    [BsonId]
    public Guid ID { get; set; } = Guid.NewGuid();

    [BsonElement("discord")]
    public ulong? DiscordID { get; set; }

    [BsonElement("created")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    [BsonElement("sender")]
    public long SenderID { get; init; }

    [BsonElement("content")]
    public string Content { get; init; } = string.Empty;

    [BsonElement("channel")]
    public string Channel { get; init; } = string.Empty;

    [BsonElement("deleted")]
    public bool Deleted { get; set; }

    [BsonIgnore]
    public RequestCache Cache { get; set; } = new();

    public APIChatMessage ToAPI() => new()
    {
        ID = ID.ToString(),
        CreatedAtUnix = CreatedAt.ToUnixTimeSeconds(),
        Content = Content,
        Channel = Channel,
        Sender = Cache.Users.Get(SenderID)?.ToAPI() ?? APIUser.CreateUnknown(SenderID)
    };
}
