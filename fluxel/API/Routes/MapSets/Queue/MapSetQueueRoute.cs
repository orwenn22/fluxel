using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Maps;
using Midori.Networking;

namespace fluxel.API.Routes.MapSets.Queue;

public class MapSetQueueRoute : IFluxelAPIRoute
{
    public string RoutePath => "/mapsets/queue";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var limit = interaction.GetIntQuery("limit") ?? 50;
        var offset = interaction.GetIntQuery("offset") ?? 0;

        var queue = MapSetHelper.AllInQueue;
        var count = queue.Count;
        queue = queue.OrderBy(x => x.QueueTime).ToList();
        queue.ForEach(x => x.Cache = interaction.Cache);

        limit = Math.Clamp(limit, 1, 50);
        queue = queue.Skip(offset).Take(limit).ToList();

        interaction.SetPaginationInfo(limit, offset, count, queue.Count);
        await interaction.Reply(HttpStatusCode.OK, queue.Select(x => x.ToAPI(MapSetInclude.QueueInfo)));
    }
}
