using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Clubs;
using fluxel.Models.Other;
using fluxel.Utils;
using fluXis.Online.API.Payloads.Clubs;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Clubs;

public class CreateClubRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/clubs";
    public HttpMethod Method => HttpMethod.Post;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (interaction.User.Club != null)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "You are already in a club");
            return;
        }

        if (!interaction.TryParseBody<CreateClubPayload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        // validate parameters

        if (string.IsNullOrEmpty(payload.Name) || payload.Name.Length is < 3 or > 24)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Name has to be between 3 and 24 characters");
            return;
        }

        if (string.IsNullOrEmpty(payload.Tag) || !payload.Tag.Validate(StringValidator.ValidationType.ClubTag))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Tag has to be between 3-5 characters and may only contain alphanumeric characters.");
            return;
        }

        if (!isValidHex(payload.ColorStart) || !isValidHex(payload.ColorEnd))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Invalid hex color code");
            return;
        }

        // check if club with name or tag already exists

        if (ClubHelper.ByName(payload.Name) != null)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Club with this name already exists");
            return;
        }

        payload.Tag = payload.Tag.ToUpperInvariant();

        if (ClubHelper.ByTag(payload.Tag) != null)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Club with this tag already exists");
            return;
        }

        var iconBytes = Array.Empty<byte>();
        var bannerBytes = Array.Empty<byte>();

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

            iconBytes = bytes;
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

            bannerBytes = bytes;
        }

        // create club

        var club = new Club
        {
            Name = payload.Name,
            Tag = payload.Tag,
            JoinType = payload.JoinType,
            OwnerID = interaction.User.ID,
            Colors = new List<GradientColor>
            {
                new() { Color = payload.ColorStart, Position = 0 },
                new() { Color = payload.ColorEnd, Position = 1 }
            },
            Members = new List<long> { interaction.User.ID }
        };

        await interaction.Reply(HttpStatusCode.Created, club.ToAPI());

        if (iconBytes.Length != 0)
            club.IconHash = Assets.WriteHashedImage(AssetType.ClubIcon, iconBytes);
        if (bannerBytes.Length != 0)
            club.BannerHash = Assets.WriteHashedImage(AssetType.ClubBanner, bannerBytes);

        ClubHelper.Add(club);
    }

    private static bool isValidHex(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return false;

        return Regex.IsMatch(hex, "^#(?:[0-9a-fA-F]{3}){1,2}$");
    }
}
