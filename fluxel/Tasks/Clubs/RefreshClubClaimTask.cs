using System;
using System.Linq;
using fluxel.Database.Helpers;

namespace fluxel.Tasks.Clubs;

public class RefreshClubClaimTask : IBasicTask
{
    public string Name => $"RefreshClubClaim({id})";

    private long id { get; }

    public RefreshClubClaimTask(long id)
    {
        this.id = id;
    }

    public void Run()
    {
        var map = MapHelper.Get(id);

        if (map == null)
            throw new ArgumentException($"No map with id {id} was found!");

        var claim = ClubHelper.GetClaim(map.ID, true)!;

        var scores = ClubHelper.GetScoresOnMap(map.ID);
        scores = scores.OrderByDescending(s => s.PerformanceRating).ToList();

        claim.ClubID = scores.FirstOrDefault()?.ClubID ?? 0;
        ClubHelper.UpdateClaim(claim);
    }
}
