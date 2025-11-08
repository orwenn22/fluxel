using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using Midori.Logging;
using Midori.Networking;

namespace fluxel.API.Routes.Invites;

public class GetInviteRoute : IFluxelAPIRoute
{
    public string RoutePath => "/invites/:code";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetStringParameter("code", out var code))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("code", "string"));
            return;
        }

        Logger.Log(code);
        var invite = ClubHelper.GetInvite(code);

        if (invite == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "The provided invite code is invalid.");
            return;
        }

        var club = ClubHelper.Get(invite.ClubID);

        if (club == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "The club this invite goes to doesn't exist.");
            return;
        }

        await interaction.Reply(HttpStatusCode.OK, new
        {
            code = invite.InviteCode,
            club = club.ToAPI(),
            user = invite.UserID
        });
    }
}
