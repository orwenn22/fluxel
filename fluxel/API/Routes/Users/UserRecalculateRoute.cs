using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Tasks.Users;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Users;

public class UserRecalculateRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/user/:id/recalculate";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!interaction.User.IsDeveloper())
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, ResponseStrings.NoPermission);
            return;
        }

        ServerHost.Instance.Scheduler.Schedule(new RecalculateUserTask(id));
        await interaction.Reply(HttpStatusCode.OK);
    }
}
