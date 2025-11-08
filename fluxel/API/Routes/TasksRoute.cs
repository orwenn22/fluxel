using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes;

public class TasksRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/tasks";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.User.IsDeveloper())
        {
            await interaction.Reply(HttpStatusCode.Unauthorized, ResponseStrings.NoPermission);
            return;
        }

        // copy to avoid modification issues while serializing
        var tasks = ServerHost.Instance.Scheduler.Queue.ToList();
        await interaction.Reply(HttpStatusCode.OK, new
        {
            count = tasks.Count,
            tasks = tasks.Select(x => x.ToString())
        });
    }
}
