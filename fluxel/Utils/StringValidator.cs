using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace fluxel.Utils;

public static class StringValidator
{
    public static bool IsBlacklisted(this string input)
    {
        var path = Path.Combine("username-blacklist.txt");

        if (!File.Exists(path))
            return false;

        var file = File.ReadAllLines(path);
        var split = file.Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();

        var isInSplit = split.Any(input.ContainsLower);
        return isInSplit;
    }

    public static bool Validate(this string input, ValidationType type) => type switch
    {
        ValidationType.Username => validateUsername(input),
        ValidationType.DisplayName => validateDisplayName(input),
        ValidationType.ClubTag => validateClubTag(input),
        ValidationType.Twitter => validateTwitter(input),
        ValidationType.Discord => validateDiscord(input),
        ValidationType.Twitch => validateTwitch(input),
        ValidationType.YouTube => validateYouTube(input),
        _ => false
    };

    private static bool validateUsername(string input)
        => Regex.IsMatch(input, "^[a-zA-Z0-9_]{3,16}$");

    private static bool validateDisplayName(string input)
        => input.Length is >= 2 and <= 20;

    private static bool validateClubTag(string input)
        => Regex.IsMatch(input, "^[A-Z0-9]{3,5}$");

    public static bool ValidateArtistID(string id)
        => Regex.IsMatch(id, "^[a-z0-9-]{1,32}$");

    private static bool validateTwitter(string input) => input.Length <= 15;
    private static bool validateDiscord(string input) => input.Length is <= 32 and >= 2;
    private static bool validateTwitch(string input) => input.Length is <= 25 and >= 4;
    private static bool validateYouTube(string input) => input.Length <= 30;

    public enum ValidationType
    {
        Username,
        DisplayName,
        ClubTag,
        Twitter,
        Discord,
        Twitch,
        YouTube
    }
}
