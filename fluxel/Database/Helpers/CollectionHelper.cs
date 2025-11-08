using fluxel.Models.Collections;
using MongoDB.Bson;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class CollectionHelper
{
    private static IMongoCollection<Collection> collection => MongoDatabase.GetCollection<Collection>("collections");

    public static void Add(Collection col) => collection.InsertOne(col);

    public static Collection? Get(string id) => !ObjectId.TryParse(id, out var obj) ? null : Get(obj);
    public static Collection? Get(ObjectId id) => collection.Find(x => x.ID == id).FirstOrDefault();
}
