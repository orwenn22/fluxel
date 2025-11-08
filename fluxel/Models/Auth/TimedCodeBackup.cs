using Midori.Utils;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Auth;

public class TimedCodeBackup
{
    [BsonElement("code")]
    public string Code { get; init; } = null!;

    [BsonElement("used")]
    public bool Used { get; init; } = false;

    public static TimedCodeBackup Generate()
    {
        var part1 = RandomizeUtils.GenerateRandomString(6);
        var part2 = RandomizeUtils.GenerateRandomString(6);
        return new TimedCodeBackup { Code = $"{part1}-{part2}" };
    }
}
