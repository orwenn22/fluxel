using System.Collections.Generic;
using fluxel.Models.Groups;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class GroupHelper
{
    private static IMongoCollection<Group> collection => MongoDatabase.GetCollection<Group>("groups");

    public static List<Group> All => collection.Find(m => true).ToList();

    public static Group? Get(string id) => collection.Find(m => m.ID == id).FirstOrDefault();

    public static void Add(Group group) => collection.InsertOne(group);
}
