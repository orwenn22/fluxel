using System;
using System.Collections.Generic;
using fluXis.Online.API.Models.Chat;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace fluxel.Models.Chat;

[JsonObject(MemberSerialization.OptIn)]
public class ChatChannel
{
    [BsonId]
    public string Name { get; init; } = null!;

    [BsonElement("type")]
    public APIChannelType Type { get; init; }

    [BsonElement("users")]
    public List<long> Users { get; init; } = new();

    public ChatChannel(string name, APIChannelType type)
    {
        Name = name;
        Type = type;
    }

    [BsonConstructor]
    [Obsolete("This is for bson parsing only.")]
    public ChatChannel()
    {
    }
}
