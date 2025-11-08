using System.Reflection;
using Newtonsoft.Json;

namespace fluxel.Constants;

public static class ResponseStrings
{
    public static string NoToken => MissingHeader("token");
    public static string InvalidToken => "The provided token is invalid.";
    public static string TokenUserNotFound => "The user associated with the provided token was not found.";

    public static string NoPermission => "You do not have required permissions to perform this action.";

    public static string FieldRequired => "This field is required.";

    public static string InvalidBodyJson => "The provided body is not valid JSON.";
    public static string BodyMissingProperty(string property) => $"Body is missing the '{property}' property.";

    public static string InvalidBody => "The provided body is invalid.";

    public static string MapNotFound => ProvidedIDNotFound("map");
    public static string MapHashNotFound => ProvidedTypeNotFound("map", "hash");
    public static string MapSetNotFound => ProvidedIDNotFound("mapset");
    public static string UserNotFound => ProvidedIDNotFound("user");
    public static string ScoreNotFound => ProvidedIDNotFound("score");
    public static string ClubNotFound => ProvidedIDNotFound("club");
    public static string GroupNotFound => ProvidedIDNotFound("group");
    public static string MessageNotFound => ProvidedIDNotFound("message");
    public static string LobbyNotFound => ProvidedIDNotFound("lobby");
    public static string EmailNotFound => ProvidedTypeNotFound("user", "email");

    public static string ProvidedIDNotFound(string field) => ProvidedTypeNotFound(field, "ID");
    public static string ProvidedTypeNotFound(string field, string type) => $"No {field} with the provided {type} was found.";

    public static string MissingHeader(string header) => $"The '{header}' header is missing.";
    public static string InvalidHeader(string header) => $"The '{header}' header is invalid.";

    public static string InvalidParameter(string parameter, string type) => $"The parameter '{parameter}' is not a valid {type}.";

    public static string AlreadyInLobby => "You are already in a lobby.";
    public static string NotInLobby => "You are not in a lobby.";
    public static string MapHashMismatch => "The provided map hash does not match the server's hash.";
    public static string NotLobbyHost => "You are not the host of this lobby.";

    public static string MissingJsonField<T>(string name)
    {
        var type = typeof(T);
        var field = type.GetField(name) as MemberInfo ?? type.GetProperty(name);

        var jsonName = name;

        if (field != null)
        {
            var prop = field.GetCustomAttribute<JsonPropertyAttribute>();
            jsonName = prop?.PropertyName ?? name;
        }

        return BodyMissingProperty(jsonName);
    }

    #region Map Voting

    public static string CannotRateOwnMap => "You cannot rate your own map.";
    public static string AlreadyVoted => "You have already voted for this map.";

    #endregion
}
