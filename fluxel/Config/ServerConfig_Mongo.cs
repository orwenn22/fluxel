namespace fluxel.Config;

public partial class ServerConfig
{
    public class MongoConfig
    {
        public string Connection { get; init; } = "mongodb://localhost:27017";
        public string Database { get; init; } = "fluxel";
    }
}
