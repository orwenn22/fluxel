using System;
using fluxel.Models;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class CounterHelper
{
    private static IMongoCollection<Counter> collection => MongoDatabase.GetCollection<Counter>("counters");

    private static readonly object thread_lock = new();

    public static void Add(CounterType type, Func<long> current)
    {
        lock (thread_lock)
        {
            var counter = collection.Find(c => c.Type == type).FirstOrDefault();

            if (counter is not null)
                return;

            counter = new Counter
            {
                Type = type,
                Value = current() + 1
            };

            collection.InsertOne(counter);
        }
    }

    public static long GetNext(CounterType type)
    {
        lock (thread_lock)
        {
            var counter = collection.Find(c => c.Type == type).FirstOrDefault();

            if (counter is null)
                throw new ArgumentException($"Counter {type} has not been initialized!");

            var num = counter.GetAndIncrease();
            collection.ReplaceOne(x => x.Type == type, counter);
            return num;
        }
    }
}
