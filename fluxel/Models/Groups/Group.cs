using System.Linq;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluXis.Online.API.Models.Groups;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace fluxel.Models.Groups;

[JsonObject(MemberSerialization.OptIn)]
public class Group
{
    [BsonId]
    public string ID { get; init; } = "";

    /// <summary>
    /// The full name of the group.
    /// </summary>
    [BsonElement("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// The 3-letter tag of the group.
    /// </summary>
    [BsonElement("tag")]
    public string Tag { get; set; } = "";

    /// <summary>
    /// The color of the group.
    /// </summary>
    [BsonElement("color")]
    public string Color { get; set; } = "#ffffff";

    public APIGroup ToAPI(bool members = false) => new()
    {
        ID = ID,
        Color = Color,
        Name = Name,
        Tag = Tag,
        Members = members ? UserHelper.InGroup(ID).Select(u => u.ToAPI()) : null
    };
}
