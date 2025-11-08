using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using fluxel.Models;
using fluxel.Models.Maps;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class MapHelper
{
    private static IMongoCollection<Map> maps => MongoDatabase.GetCollection<Map>("maps");
    private static IMongoCollection<MapRateVote> votes => MongoDatabase.GetCollection<MapRateVote>("rate-votes");

    public static List<Map> All => maps.Find(m => true).ToList();
    public static long PureCount => MapSetHelper.AllPure.Sum(x => x.Maps.Count());

    public static void Add(Map map)
    {
        map.ID = CounterHelper.GetNext(CounterType.Map);
        maps.InsertOne(map);
    }

    public static void Remove(Map map)
    {
        var filter = Builders<Map>.Filter.Eq(m => m.ID, map.ID);
        maps.DeleteOne(filter);
    }

    public static Map? Get(long id) => maps.Find(m => m.ID == id).FirstOrDefault();
    public static Map? Get(Expression<Func<Map, bool>> filter) => maps.Find(filter).FirstOrDefault();
    public static Map? GetByHash(string hash) => maps.Find(m => m.SHA256Hash == hash).FirstOrDefault();

    public static bool TryGetMap(long id, [NotNullWhen(true)] out Map? map)
    {
        map = Get(id);
        return map != null;
    }

    public static List<Map> GetByMapper(long uid) => maps.Find(m => m.MapperID == uid).ToList();

    public static void DeleteBySet(long id)
    {
        var filter = Builders<Map>.Filter.Eq(m => m.SetID, id);
        maps.DeleteMany(filter);
    }

    public static void Update(Map map)
    {
        var filter = Builders<Map>.Filter.Eq(m => m.ID, map.ID);
        maps.ReplaceOne(filter, map);
    }

    public static void QuickUpdate(long id, Action<Map>? action)
    {
        var u = Get(id);

        if (u is null)
            throw new ArgumentNullException(nameof(id), "No map with the provided ID found.");

        action?.Invoke(u);
        Update(u);
    }

    public static void AddVote(long user, long map, float rating, float read, float track, float percept, bool purifier)
    {
        var vote = new MapRateVote
        {
            UserID = user,
            MapID = map,
            BaseRating = (float)Math.Round(Math.Clamp(rating, 0, 20), 1),
            ReadingRating = (float)Math.Round(Math.Clamp(read, 0, 5), 1),
            TrackingRating = (float)Math.Round(Math.Clamp(track, 0, 5), 1),
            PerceptionRating = (float)Math.Round(Math.Clamp(percept, 0, 5), 1),
            PurifierVote = purifier
        };

        votes.InsertOne(vote);
    }

    public static void ClearVotes(long map) => votes.DeleteMany(x => x.MapID == map);
    public static bool HasVoted(long user, long map) => votes.Find(m => m.UserID == user && m.MapID == map).Any();
    public static List<MapRateVote> GetVotesByMap(long map) => votes.Find(m => m.MapID == map).ToList();
}
