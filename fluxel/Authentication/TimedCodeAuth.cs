using OtpNet;

namespace fluxel.Authentication;

public static class TimedCodeAuth
{
    public static bool Verify(string key, string code)
    {
        var totp = create(key);
        return totp.VerifyTotp(code, out _);
    }

    private static Totp create(string key)
    {
        var b32 = Base32Encoding.ToBytes(key);
        return new Totp(b32);
    }
}
