using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models;

public class Counter
{
    [BsonId]
    public CounterType Type { get; set; }

    [BsonElement("value")]
    public long Value { get; set; }

    public long GetAndIncrease() => Value++;
}

public enum CounterType
{
    Club = 0,
    Map = 1,
    MapSet = 2,
    Score = 3,
    User = 4
}
