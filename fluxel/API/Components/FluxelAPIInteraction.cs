using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluxel.Models.Users;
using fluxel.Utils;
using Midori.API;
using Midori.API.Components;
using Midori.API.Components.Interfaces;
using Midori.API.Components.Json;
using Midori.Logging;
using Midori.Networking;
using Midori.Utils;

namespace fluxel.API.Components;

public class FluxelAPIInteraction : JsonInteraction<FluxelAPIResponse>, IHasAuthorizationInfo
{
    protected override string[] AllowedHeaders => base.AllowedHeaders.Concat(extra_headers).ToArray();
    protected override string[] AllowedMethods => base.AllowedMethods.Concat(extra_methods).ToArray();

    private static readonly string[] extra_headers =
    {
        "X-Multifactor-Token"
    };

    private static readonly string[] extra_methods =
    {
        "PATCH"
    };

    /// <summary>
    /// Cache that is only valid for the current request
    /// Used to store data that is requested multiple times in the same request
    /// </summary>
    public RequestCache Cache { get; } = new();

    public long UserID { get; private set; } = -1;
    public User User { get; private set; } = null!;

    protected override bool RespondOnInvalidParameter => false;

    public bool IsAuthorized => UserID != -1;
    public string AuthorizationError { get; private set; } = string.Empty;

    public bool HasValidMfa { get; private set; }

    public Version? GameVersion { get; private set; }
    public bool IsDevelopmentBuild { get; private set; }

    private Dictionary<string, object>? cache;
    private Dictionary<string, string>? errors;

    protected override void OnPopulate()
    {
        parseVersion();

        var token = Request.Headers["Authorization"];
        // token ??= Request.Cookies["token"]?.Value;

        if (string.IsNullOrEmpty(token))
        {
            AuthorizationError = ResponseStrings.NoToken;
            return;
        }

        token = token.Split(" ").Last().Trim();
        var session = SessionHelper.Get(token);

        if (session == null)
        {
            AuthorizationError = ResponseStrings.InvalidToken;
            return;
        }

        var user = Cache.Users.Get(session.UserID);

        if (user == null)
        {
            AuthorizationError = ResponseStrings.TokenUserNotFound;
            return;
        }

        UserID = session.UserID;
        User = user;

        var mfaToken = Request.Headers["X-Multifactor-Token"];
        // mfaToken ??= Request.Cookies["last-mfa"]?.Value;

        if (string.IsNullOrWhiteSpace(mfaToken))
            return;

        HasValidMfa = AuthHelper.IsValidToken(UserID, mfaToken);
    }

    private void parseVersion()
    {
        var header = Request.Headers["X-Version"];

        // default
        GameVersion = new Version(0, 0, 0);

        if (header == null)
            return;

        if (header.EqualsLower("local development build"))
        {
            IsDevelopmentBuild = true;
            return;
        }

        if (Version.TryParse(header, out var ver))
            GameVersion = ver;
    }

    public void AddError(string field, string reason)
    {
        errors ??= new Dictionary<string, string>();
        errors[field] = reason;
    }

    public void AddCache(string key, object obj)
    {
        cache ??= new Dictionary<string, object>();
        cache[key] = obj;
    }

    public bool TryGetCache<T>(string key, [NotNullWhen(true)] out T? obj)
    {
        obj = default;

        if (cache is null)
            return false;

        try
        {
            obj = (T)cache[key];
            return obj != null;
        }
        catch
        {
            return false;
        }
    }

    public bool TryGetUserID(string key, out long id)
    {
        id = -1;

        var val = GetStringParameter(key) ?? throw new InvalidOperationException($"Key '{key}' does not exist on the current route.");
        val = val.ToLowerInvariant();

        if (val == "@me")
        {
            if (!IsAuthorized)
            {
                ReplyMessage(HttpStatusCode.Unauthorized, "You need to be authorized to use '@me' as a substitute for user IDs.");
                return false;
            }

            id = UserID;
            return true;
        }

        if (long.TryParse(val, out id))
            return true;

        ReplyMessage(HttpStatusCode.BadRequest, DefaultResponseStrings.InvalidParameter(key, "long"));
        return false;
    }

    public override Task HandleRoute<T>(IAPIRoute<T> route)
    {
        if (route is IFluxelAPIRoute fr)
        {
            var errs = fr.Validate(this).ToList();

            if (errs.Count == 0)
                return base.HandleRoute(route);

            foreach (var (field, reason) in errs)
                AddError(field, reason);

            return ReplyMessage(HttpStatusCode.BadRequest, $"{errs[0].Item1}: {errs[0].Item2}");
        }

        throw new InvalidOperationException($"Expected {typeof(IFluxelAPIRoute)} but received {route.GetType()}.");
    }

    public bool TryParseBody<T>([NotNullWhen(true)] out T? result)
    {
        result = default!;

        if (Request.BodyStream.Length <= 0)
            return false;

        var body = new StreamReader(Request.BodyStream).ReadToEnd();

        try
        {
            result = body.Deserialize<T>();
            return result != null;
        }
        catch (Exception ex)
        {
            Logger.Add("failed to parse json", LogLevel.Error, ex);
            return false;
        }
    }

    protected override Task ReplyJson(FluxelAPIResponse response)
    {
        response.Errors = errors;
        return base.ReplyJson(response);
    }
}
