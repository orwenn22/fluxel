using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Clubs;
using fluxel.Utils;
using fluXis.Online.API.Models.Clubs;
using fluXis.Online.API.Payloads.Clubs;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Clubs;

public class ClubEditRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/club/:id";
    public HttpMethod Method => HttpMethod.Patch;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        var club = ClubHelper.Get(id);

        if (club == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.ClubNotFound);
            return;
        }

        if (club.OwnerID != interaction.UserID && !interaction.User.IsModerator())
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, ResponseStrings.NoPermission);
            return;
        }

        if (!interaction.TryParseBody<EditClubPayload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        var anyChanges = false;

        if (!string.IsNullOrWhiteSpace(payload.Name))
        {
            if (payload.Name.Length is < 3 or > 24)
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Name has to be between 3 and 24 characters");
                return;
            }

            club.Name = payload.Name;
            anyChanges = true;
        }

        if (payload.JoinType != null)
        {
            if (!Enum.IsDefined(typeof(ClubJoinType), payload.JoinType))
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Invalid join type.");
                return;
            }

            club.JoinType = payload.JoinType.Value;
            anyChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(payload.ColorStart))
        {
            if (!isValidHex(payload.ColorStart))
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Invalid hex color code for 'color-start'.");
                return;
            }

            club.Colors.First().Color = payload.ColorStart;
            anyChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(payload.ColorEnd))
        {
            if (!isValidHex(payload.ColorEnd))
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Invalid hex color code for 'color-end'.");
                return;
            }

            club.Colors.Last().Color = payload.ColorEnd;
            anyChanges = true;
        }

        if (!string.IsNullOrEmpty(payload.Icon))
        {
            var bytes = Convert.FromBase64String(payload.Icon);

            if (bytes.Length > Assets.MAX_IMAGE_SIZE)
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Icon is bigger than 3MB.");
                return;
            }

            if (!bytes.IsImage())
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Icon is not a valid image.");
                return;
            }

            club.IconHash = Assets.WriteHashedImage(AssetType.ClubIcon, bytes);
        }

        if (!string.IsNullOrEmpty(payload.Banner))
        {
            var bytes = Convert.FromBase64String(payload.Banner);

            if (bytes.Length > Assets.MAX_IMAGE_SIZE)
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Banner is bigger than 3MB.");
                return;
            }

            if (!bytes.IsImage())
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Banner is not a valid image.");
                return;
            }

            club.BannerHash = Assets.WriteHashedImage(AssetType.ClubBanner, bytes);
        }

        ClubHelper.Update(club);

        var includes = new List<ClubIncludes> { ClubIncludes.JoinType };
        await interaction.Reply(anyChanges ? HttpStatusCode.OK : HttpStatusCode.NotModified, club.ToAPI(includes));
    }

    private static bool isValidHex(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return false;

        return Regex.IsMatch(hex, "^#(?:[0-9a-fA-F]{3}){1,2}$");
    }
}
