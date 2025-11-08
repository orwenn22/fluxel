using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes.Groups;

public class GroupRoute : IFluxelAPIRoute
{
    public string RoutePath => "/group/:id";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetStringParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "string"));
            return;
        }

        var group = GroupHelper.Get(id);

        if (group == null)
        {
            await interaction.Reply(HttpStatusCode.NotFound, ResponseStrings.GroupNotFound);
            return;
        }

        await interaction.Reply(HttpStatusCode.OK, group.ToAPI(true));
    }
}
