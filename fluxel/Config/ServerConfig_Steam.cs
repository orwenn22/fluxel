namespace fluxel.Config;

public partial class ServerConfig
{
    public class SteamConfig
    {
        public uint AppID { get; init; }
        public string WebKey { get; init; } = null!;
    }
}
