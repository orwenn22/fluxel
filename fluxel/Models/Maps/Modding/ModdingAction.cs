using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluXis.Online.API.Models.Maps.Modding;
using fluXis.Online.API.Models.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Maps.Modding;

public class ModdingAction
{
    [BsonId]
    public ObjectId ID { get; init; }

    [BsonElement("mapset")]
    public long MapSetID { get; set; }

    [BsonElement("user")]
    public long UserID { get; set; }

    [BsonElement("type")]
    public APIModdingActionType Type { get; set; }

    [BsonElement("content")]
    public string? Content { get; set; }

    [BsonElement("time")]
    public long Time { get; set; }

    public object ToAPI(RequestCache? cache)
    {
        cache ??= new RequestCache();

        return new APIModdingAction(
            ID.ToString(),
            cache.Users.Get(UserID)?.ToAPI() ?? APIUser.CreateUnknown(UserID),
            Type, APIModdingActionState.Pending, Content, Time
        );
    }
}
