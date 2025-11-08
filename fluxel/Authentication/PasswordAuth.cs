namespace fluxel.Authentication;

public abstract class PasswordAuth
{
    public static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public static bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    public static bool IsValid(string password)
    {
        return password.Length is >= 8 and <= 32;
    }
}
