using fluxel.Models.Users.Equipment;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class UserEquipmentHelper
{
    private static IMongoCollection<NamePaint> paints => MongoDatabase.GetCollection<NamePaint>("paints");

    public static void Add(NamePaint paint) => paints.InsertOne(paint);
    public static NamePaint? Get(string id) => paints.Find(p => p.ID == id).FirstOrDefault();

    public static void AddIfMissing(NamePaint paint)
    {
        if (Get(paint.ID) is not null)
            return;

        Add(paint);
    }
}
