using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Models.Maps;
using fluxel.Search;
using fluxel.Search.Filters;
using Midori.Networking;
using osu.Framework.Extensions.EnumExtensions;

namespace fluxel.API.Routes.MapSets;

public class MapSetListRoute : IFluxelAPIRoute
{
    public string RoutePath => "/mapsets";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var limit = interaction.GetIntQuery("limit") ?? 50;
        var offset = interaction.GetIntQuery("offset") ?? 0;
        var query = interaction.GetStringQuery("q") ?? string.Empty;

        limit = Math.Clamp(limit, 1, 50);

        var all = interaction.Cache.MapSets.All;
        interaction.Cache.Maps.EnsureAll();

        var filter = new MapSetSearchFilter();
        SearchParser.Parse<MapSetSearchFilter, MapSet>(filter, query);

        if (interaction.TryGetIntQuery("status", out var status))
        {
            if (status <= 10)
            {
                if (!Enum.IsDefined((MapStatus)status))
                {
                    await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Invalid status provided.");
                    return;
                }

                filter.Status = status switch
                {
                    -1 or 0 => StatusFlags.Unsubmitted,
                    1 => StatusFlags.Pending,
                    2 => StatusFlags.Impure,
                    3 or 4 => StatusFlags.Pure,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            else
                filter.Status = null;
        }

        var sets = all.Where(s => filter.Match(s) && !s.InternalFlags.HasFlagFast(InternalSetFlags.ShadowBan))
                      .OrderByDescending(x => filter.Status == StatusFlags.Pure ? x.DateRanked : x.Submitted).Skip(offset).Take(limit).Select(x => x.ToAPI()).ToList();

        interaction.SetPaginationInfo(limit, offset, all.Count, sets.Count);
        await interaction.Reply(HttpStatusCode.OK, sets);
    }
}
