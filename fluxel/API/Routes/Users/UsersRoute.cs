using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Users;
using fluxel.Utils;
using Midori.Networking;

namespace fluxel.API.Routes.Users;

public class UsersRoute : IFluxelAPIRoute
{
    public string RoutePath => "/users";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var limit = interaction.GetIntQuery("limit") ?? 50;
        var offset = interaction.GetIntQuery("offset") ?? 0;
        var name = interaction.GetStringQuery("name") ?? "";
        var with = (interaction.GetStringQuery("with") ?? "").Split(",");

        limit = Math.Clamp(limit, 1, 100);

        var all = UserHelper.All;
        all.ForEach(x => x.Cache = interaction.Cache);

        UserIncludes include = 0;

        foreach (var se in with)
        {
            switch (se)
            {
                case "creation":
                    include |= UserIncludes.CreatedAt;
                    break;

                case "login":
                    include |= UserIncludes.LastLogin;
                    break;

                case "flags" when interaction.IsAuthorized && interaction.User.IsModerator():
                    include |= UserIncludes.Flags;
                    break;
            }
        }

        var users = all.Skip(offset).Where(n => string.IsNullOrWhiteSpace(name) || n.Username.ContainsLower(name) || (n.DisplayName?.ContainsLower(name) ?? false)).Take(limit)
                       .Select(x => x.ToAPI(include: include));

        interaction.SetPaginationInfo(limit, offset, all.Count, users.Count());
        await interaction.Reply(HttpStatusCode.OK, users);
    }
}
