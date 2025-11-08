using System;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Extensions;
using fluxel.Database.Helpers;
using fluxel.Models.Users;
using fluxel.Utils;
using fluXis.Online.API.Payloads.Users;
using Midori.API.Components.Interfaces;
using Midori.Networking;

namespace fluxel.API.Routes.Users;

public class UserUpdateRoute : IFluxelAPIRoute, INeedsAuthorization
{
    public string RoutePath => "/user/:id";
    public HttpMethod Method => HttpMethod.Patch;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        if (!UserHelper.TryGet(id, out var user))
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.UserNotFound);
            return;
        }

        if (user.ID != interaction.UserID && !interaction.User.IsModerator())
        {
            await interaction.ReplyMessage(HttpStatusCode.Forbidden, ResponseStrings.NoPermission);
            return;
        }

        if (!interaction.TryParseBody<UserProfileUpdatePayload>(out var payload))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidBodyJson);
            return;
        }

        #region Validations

        (string? hash, bool animated) avatar = (user.AvatarHash, user.HasAnimatedAvatar);
        (string? hash, bool animated) banner = (user.BannerHash, user.HasAnimatedBanner);

        if (!string.IsNullOrEmpty(payload.Avatar))
        {
            var bytes = Convert.FromBase64String(payload.Avatar);

            if (bytes.Length > Assets.MAX_IMAGE_SIZE)
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Avatar is bigger than 3MB.");
                return;
            }

            if (!bytes.IsImage())
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Avatar is not a valid image.");
                return;
            }

            var gif = bytes.IsGif();

            if (gif && !interaction.User.IsSupporter)
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "You are not allowed to use animated avatars.");
                return;
            }

            var hash = gif ? Assets.WriteAnimatedImage(AssetType.Avatar, bytes) : Assets.WriteHashedImage(AssetType.Avatar, bytes);

            if (string.IsNullOrWhiteSpace(hash))
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Failed to update avatar!");
                return;
            }

            avatar = (hash, gif);
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

            var gif = bytes.IsGif();

            if (gif && !interaction.User.IsSupporter)
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "You are not allowed to use animated banners.");
                return;
            }

            var hash = gif ? Assets.WriteAnimatedImage(AssetType.Banner, bytes) : Assets.WriteHashedImage(AssetType.Banner, bytes);

            if (string.IsNullOrWhiteSpace(hash))
            {
                await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Failed to update banner!");
                return;
            }

            banner = (hash, gif);
        }

        if (payload.DisplayName != null && !payload.DisplayName.Validate(StringValidator.ValidationType.DisplayName) && payload.DisplayName.Length != 0)
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Invalid display name length or characters. Must be between 2 and 20 characters.");
            return;
        }

        if (payload.AboutMe is { Length: > 512 })
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "AboutMe is too long. Maximum length is 512 characters.");
            return;
        }

        if (payload.Pronouns is { Length: > 16 })
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Pronouns are too long. Maximum length is 16 characters.");
            return;
        }

        if (payload.Discord != null && !payload.Discord.Validate(StringValidator.ValidationType.Discord))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Discord username is invalid.");
            return;
        }

        if (payload.Twitch != null && !payload.Twitch.Validate(StringValidator.ValidationType.Twitch))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Twitch handle is invalid.");
            return;
        }

        if (payload.Twitter != null && !payload.Twitter.Validate(StringValidator.ValidationType.Twitter))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "Twitter handle is invalid.");
            return;
        }

        if (payload.YouTube != null && !payload.YouTube.Validate(StringValidator.ValidationType.YouTube))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, "YouTube handle is invalid.");
            return;
        }

        #endregion

        user = UserHelper.UpdateLocked(user.ID, u =>
        {
            u.AvatarHash = avatar.hash;
            u.HasAnimatedAvatar = avatar.animated;
            u.BannerHash = banner.hash;
            u.HasAnimatedBanner = banner.animated;

            u.DisplayName = payload.DisplayName ?? u.DisplayName;
            u.AboutMe = payload.AboutMe ?? u.AboutMe;
            u.Pronouns = payload.Pronouns ?? u.Pronouns;
            u.Socials.Discord = payload.Discord ?? u.Socials.Discord;
            u.Socials.Twitch = payload.Twitch ?? u.Socials.Twitch;
            u.Socials.Twitter = payload.Twitter ?? u.Socials.Twitter;
            u.Socials.YouTube = payload.YouTube ?? u.Socials.YouTube;
        });

        await interaction.Reply(HttpStatusCode.OK, user.ToAPI(include: UserIncludes.Socials));
    }
}
