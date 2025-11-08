using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Maps;
using fluxel.Models.Scores;
using fluXis.Online.API.Responses.Maps;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Maps;

public class MapLeaderboardRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/map/:id/scores";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        var map = MapHelper.Get(id);

        if (map == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapNotFound);
            return;
        }

        var set = map.MapSet;

        if (set == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapSetNotFound);
            return;
        }

        var type = interaction.GetStringQuery("type") ?? "global";
        var version = interaction.GetStringQuery("version") ?? map.SHA256Hash;

        switch (type)
        {
            case "global":
            {
                var all = ScoreHelper.FromMap(map, version).ToList();
                all.ForEach(s => s.Cache = interaction.Cache);

                reply(interaction, set, map, filterList(all.OrderByDescending(s => s.PerformanceRating).ToList()));
                break;
            }

            case "country":
                if (string.IsNullOrEmpty(interaction.User.CountryCode))
                {
                    await interaction.ReplyMessage(HttpStatusCode.BadRequest, "We don't know which country you are in. oT-To");
                    return;
                }

                reply(interaction, set, map, getCountry(map, version, interaction.User.CountryCode));
                break;

            case "club":
                if (interaction.User.Club == null)
                {
                    await interaction.ReplyMessage(HttpStatusCode.BadRequest, "You are not in a club.");
                    return;
                }

                reply(interaction, set, map, getClub(map, version, interaction.User.Club.ID));
                break;

            case "friends":
            {
                var following = RelationHelper.GetFollowing(interaction.User.ID);
                following.Add(interaction.UserID);

                var all = ScoreHelper.FromMap(map, version).Where(s => following.Contains(s.UserID)).ToList();
                reply(interaction, set, map, filterList(all.OrderByDescending(s => s.PerformanceRating).ToList()));
                break;
            }

            case "clubs":
                await interaction.Reply(HttpStatusCode.OK, new MapLeaderboardClubs(map.ToAPI(), ClubHelper.GetScoresOnMap(map.ID).OrderByDescending(s => s.PerformanceRating).Select(s => s.ToAPI())));
                break;

            default:
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "The parameter 'type' is not valid. Valid types are 'global', 'country' or 'club'.");
                break;
        }
    }

    private void reply(FluxelAPIInteraction interaction, MapSet set, Map map, IEnumerable<Score> scores) => interaction.Reply(HttpStatusCode.OK, new MapLeaderboard(set.ToAPI(), map.ToAPI(),
        scores.Select(s =>
        {
            var api = s.ToAPI();

            // this is stupid
            // but also the best way
            if (interaction.UserID != -1)
                api.User.Following = RelationHelper.IsFollowing(interaction.UserID, api.User.ID);

            return api;
        })));

    private static List<Score> filterList(List<Score> all)
    {
        var scores = new List<Score>();

        foreach (var score in all)
        {
            if (scores.Count >= 50)
                break;

            if (scores.Any(s => s.UserID == score.UserID))
                continue;

            scores.Add(score);
        }

        return scores;
    }

    private List<Score> getCountry(Map map, string? version, string code)
        => filterList(ScoreHelper.FromMap(map, version)
                                 .Where(s => s.User?.CountryCode == code)
                                 .OrderByDescending(s => s.PerformanceRating).ToList());

    private List<Score> getClub(Map map, string? version, long id)
    {
        return filterList(ScoreHelper.FromMap(map, version)
                                     .Where(s => s.User?.Club?.ID == id)
                                     .OrderByDescending(s => s.PerformanceRating)
                                     .ToList());
    }
}
