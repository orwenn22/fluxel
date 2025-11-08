using fluXis.Online.API.Models.Other;

namespace fluxel.Modules.Messages;

public class UserAchievementMessage
{
    public long UserID { get; }
    public Achievement Achievement { get; }

    public UserAchievementMessage(long userID, Achievement achievement)
    {
        UserID = userID;
        Achievement = achievement;
    }
}
