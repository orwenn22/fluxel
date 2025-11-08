using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using fluxel.Database.Helpers;
using fluxel.Models.Auth;
using fluxel.Models.Users;

namespace fluxel.Authentication;

// passkey login stuff using fido2
public static class PasskeyAuth
{
    private static Fido2 fido { get; }
    private static Dictionary<Guid, CredentialCreateOptions> configCache { get; } = new();
    private static Dictionary<Guid, AssertionOptions> assertCache { get; } = new();

    static PasskeyAuth()
    {
        fido = new Fido2(new Fido2Configuration
        {
            ServerDomain = "auth.flux.moe",
            ServerName = "fluXisAuth",
            Origins = new HashSet<string> { "https://auth.flux.moe" },
            Timeout = 60000,
            TimestampDriftTolerance = 300000
        });
    }

    public static (Guid, CredentialCreateOptions) CreateConfig(User user)
    {
        var cred = fido.RequestNewCredential(user.GetFidoUser(), null!);
        var guid = Guid.NewGuid();
        configCache.Add(guid, cred);
        return (guid, cred);
    }

    public static (Guid, AssertionOptions) CreateAssertOptions(long user)
    {
        var existing = AuthHelper.GetByUserID(user);

        if (!existing.Any())
            throw new Exception("This user has no registered passkeys.");

        var exts = new AuthenticationExtensionsClientInputs()
        {
            Extensions = true,
            UserVerificationMethod = true
        };

        var assert = fido.GetAssertionOptions(existing.Select(x => x.Descriptor), UserVerificationRequirement.Discouraged, exts);
        var guid = Guid.NewGuid();

        assertCache.Add(guid, assert);
        return (guid, assert);
    }

    public static async void CreateCredential(AuthenticatorAttestationRawResponse response, Guid guid, User user)
    {
        if (!configCache.TryGetValue(guid, out var options))
            throw new Exception("Invalid guid.");

        IsCredentialIdUniqueToUserAsyncDelegate callback = static (args, _) =>
        {
            var users = AuthHelper.GetByID(args.CredentialId);
            return Task.FromResult(users.Count <= 0);
        };

        var cred = await fido.MakeNewCredentialAsync(response, options, callback);

        var stored = new Passkey()
        {
            ID = cred.Result!.CredentialId,
            Descriptor = new PublicKeyCredentialDescriptor(cred.Result.CredentialId),
            UserID = user.ID,
            UserHandle = cred.Result.User.Id,
            PublicKey = cred.Result!.PublicKey,
            SignCount = cred.Result.Counter
        };

        AuthHelper.Add(stored);
    }

    public static async Task<long> Verify(AuthenticatorAssertionRawResponse response, Guid guid)
    {
        var stored = AuthHelper.GetByID(response.Id).FirstOrDefault() ?? throw new Exception("This user has no passkey.");

        IsUserHandleOwnerOfCredentialIdAsync callback = static (args, _) =>
        {
            var storedCreds = AuthHelper.GetByHandle(args.UserHandle);
            return Task.FromResult(storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId)));
        };

        var counter = stored.SignCount;
        var assert = assertCache[guid];
        var res = await fido.MakeAssertionAsync(response, assert, stored.PublicKey, counter, callback);

        if (res.Status != "ok")
            throw new Exception(res.Status);

        stored.SignCount = res.Counter;
        AuthHelper.Update(stored);

        return stored.UserID;
    }
}
