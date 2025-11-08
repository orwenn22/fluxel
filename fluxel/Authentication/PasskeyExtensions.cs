using System.Text;
using Fido2NetLib;
using fluxel.Models.Users;

namespace fluxel.Authentication;

public static class PasskeyExtensions
{
    public static Fido2User GetFidoUser(this User user)
    {
        return new Fido2User
        {
            Name = user.Username,
            Id = Encoding.UTF8.GetBytes(user.ID.ToString())
        };
    }
}
