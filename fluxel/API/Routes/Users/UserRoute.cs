using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Users;
using Midori.Networking;

namespace fluxel.API.Routes.Users;

public class UserRoute : IFluxelAPIRoute
{
    public string RoutePath => "/user/:id";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        var mode = interaction.GetIntQuery("mode") ?? 0;

        if (mode is > 8 or < 4)
            mode = 0;

        if (!UserHelper.TryGet(id, out var user))
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.UserNotFound);
            return;
        }

        var includes = UserIncludes.CreatedAt
                       | UserIncludes.LastLogin
                       | UserIncludes.Socials
                       | UserIncludes.Statistics;

        if (interaction.IsAuthorized)
            includes |= UserIncludes.Following;
        if (interaction.UserID == id)
            includes |= UserIncludes.Email;

        await interaction.Reply(HttpStatusCode.OK, user.ToAPI(interaction.UserID, mode, includes));
    }
}
