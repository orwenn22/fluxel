using fluxel.Config;
using MongoDB.Driver;

namespace fluxel.Database;

public static class MongoDatabase
{
    private static IMongoDatabase database = null!;

    public static void Setup(ServerConfig.MongoConfig config)
    {
        var client = new MongoClient(config.Connection);
        database = client.GetDatabase(config.Database);
    }

    public static IMongoCollection<T> GetCollection<T>(string name) => database.GetCollection<T>(name);
}
