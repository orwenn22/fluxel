using System;
using System.Collections.Generic;
using fluxel.Models.Other;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class EventHelper
{
    private static IMongoCollection<StoredEvent> collection => MongoDatabase.GetCollection<StoredEvent>("events");

    public static void Add(StoredEvent ev) => collection.InsertOne(ev);

    public static List<StoredEvent> GetActive()
    {
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
        var events = collection.Find(x => x.StartTime <= now && x.EndTime > now);
        return events.ToList();
    }
}
