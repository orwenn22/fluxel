using System.Collections.Generic;
using System.Linq;
using fluXis.Online.API.Models.Other;

namespace fluxel.Constants.Achievements;

public static class AchievementList
{
    public static List<Achievement> AllOfThem => new()
    {
        SnailPace,
    };

    public static Achievement SnailPace => new()
    {
        ID = "snail-pace",
        Level = 2,
        Name = "Snail's Pace",
        Description = "This is gonna take forever..."
    };

    public static Achievement? Find(string id)
        => AllOfThem.FirstOrDefault(a => a.ID == id);
}
