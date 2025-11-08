using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluXis.Online.API.Models.Maps;
using JetBrains.Annotations;
using Midori.Networking;
using Newtonsoft.Json;

namespace fluxel.API.Routes.Leaderboards.Maps;

public class MapPlaysLeaderboardRoute : IFluxelAPIRoute
{
    public string RoutePath => "/leaderboards/maps/plays";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var list = new List<LeaderboardMap>();

        foreach (var score in ScoreHelper.All)
        {
            if (score.MapID == 0)
                continue;

            var map = interaction.Cache.Maps.Get(score.MapID);

            if (map == null)
                continue;

            var leaderboardMap = list.FirstOrDefault(m => m.Map?.ID == map.ID);

            if (leaderboardMap == null)
            {
                leaderboardMap = new LeaderboardMap
                {
                    Map = map.ToAPI(),
                    Plays = 1
                };
                list.Add(leaderboardMap);
            }
            else
            {
                leaderboardMap.Plays++;
            }
        }

        list = list.OrderByDescending(m => m.Plays).ToList();

        for (var i = 0; i < list.Count; i++)
        {
            list[i].Rank = i + 1;
        }

        await interaction.Reply(HttpStatusCode.OK, list);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private class LeaderboardMap
    {
        [JsonProperty("playcount")]
        public int Plays { get; set; }

        [JsonProperty("map")]
        public APIMap? Map { get; init; }

        [JsonProperty("rank")]
        public int Rank { get; set; }
    }
}
