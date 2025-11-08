using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Utils;
using fluXis.Online.API.Models.Maps;
using fluXis.Online.API.Models.Multi;
using fluXis.Online.API.Models.Users;
using fluXis.Scoring;
using Midori.Logging;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace fluxel.Multiplayer.Lobby;

public class ServerMultiplayerRoom
{
    public long RoomID { get; }
    public string RoomName { get; set; }
    public string Password { get; set; }
    public MultiplayerPrivacy Privacy { get; set; }

    public long HostID { get; set; }
    public long MapID { get; set; }
    public long CountdownFinishTime { get; set; }

    public List<Participant> Participants { get; } = new();
    public List<(long, int)> ScheduledScores { get; } = new();
    public List<string> CurrentMods { get; set; } = new();

    public IEnumerable<MultiplayerSocket> All => MultiplayerModule.Sockets.Where(x => HasPlayer(x.UserID));

    public readonly SemaphoreSlim RoomLock = new(1, 1);

    public ServerMultiplayerRoom(long id, long host, string name, MultiplayerPrivacy privacy, string password, long map)
    {
        RoomID = id;
        HostID = host;
        RoomName = name;
        Privacy = privacy;
        Password = password;
        MapID = map;

        // add host to player list
        Participants.Add(new Participant(host));
    }

    public async void Tick()
    {
        await RoomLock.WaitAsync();

        try
        {
            await processCountdown();

            var top = ScheduledScores.OrderByDescending(x => x.Item2).DistinctBy(x => x.Item1).ToList();
            ScheduledScores.Clear();

            foreach (var (user, score) in top)
                await All.ForEachAsync(c => c.Client.ScoreUpdated(user, score));
        }
        catch (Exception ex)
        {
            MultiplayerRoomManager.Logger.Add($"Failed to tick room {RoomID} '{RoomName}'!", LogLevel.Error, ex);
        }
        finally
        {
            RoomLock.Release();
        }
    }

    private async Task processCountdown()
    {
        if (CountdownFinishTime == 0)
            return;

        var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (Participants.Any(x => x.State != MultiplayerUserState.Ready))
        {
            await All.ForEachAsync(c => c.Client.CountdownStarted(null));
            return;
        }

        if (time < CountdownFinishTime)
            return;

        foreach (var user in Participants)
            SetPlayerStatus(user.ID, MultiplayerUserState.Playing);

        await All.ForEachAsync(c => c.Client.LoadRequested());
    }

    public void SetPlayerStatus(long id, MultiplayerUserState state)
    {
        var user = GetPlayer(id);

        if (user == null)
            return;

        user.State = state;
        All.ForEach(c => c.Client.UserStateChanged(id, state));
    }

    public void RunLocked(Action action)
    {
        RoomLock.Wait();

        try
        {
            action?.Invoke();
        }
        finally
        {
            RoomLock.Release();
        }
    }

    public void Disconnect(long id)
    {
    }

    public bool HasPlayer(long id) => Participants.Any(u => u.ID == id);
    public Participant? GetPlayer(long id) => Participants.FirstOrDefault(u => u.ID == id);

    public MultiplayerRoom ToAPI() => new()
    {
        RoomID = RoomID,
        Name = RoomName,
        Privacy = Privacy,
        Host = UserHelper.Get(HostID)?.ToAPI() ?? APIUser.CreateUnknown(HostID),
        Participants = Participants.Select(x => x.ToAPI()).ToList(),
        Map = MapHelper.Get(MapID)?.ToAPI() ?? APIMap.CreateUnknown(MapID)
    };

    public class Participant
    {
        public long ID { get; }
        public MultiplayerUserState State { get; set; } = MultiplayerUserState.Idle;
        public ScoreInfo? Score { get; set; }

        public Participant(long id)
        {
            ID = id;
        }

        public MultiplayerParticipant ToAPI() => new()
        {
            Player = UserHelper.Get(ID)?.ToAPI() ?? APIUser.CreateUnknown(ID),
            State = State
        };
    }
}
