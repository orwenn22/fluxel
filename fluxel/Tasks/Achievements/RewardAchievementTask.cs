using fluxel.Constants.Achievements;
using fluxel.Database.Helpers;
using fluxel.Modules.Messages;

namespace fluxel.Tasks.Achievements;

public class RewardAchievementTask : IBasicTask
{
    public string Name => $"RewardAchievement({uid}, {aid})";

    private long uid { get; }
    private string aid { get; }

    /// <param name="uid">user id</param>
    /// <param name="aid">achievement id</param>
    public RewardAchievementTask(long uid, string aid)
    {
        this.uid = uid;
        this.aid = aid;
    }

    public void Run()
    {
        var achievement = AchievementList.Find(aid);

        if (achievement == null)
            return;

        if (AchievementHelper.HasRewarded(aid, uid))
            return;

        ServerHost.Instance.SendMessage(new UserAchievementMessage(uid, achievement));
        AchievementHelper.Reward(aid, uid);
    }
}
