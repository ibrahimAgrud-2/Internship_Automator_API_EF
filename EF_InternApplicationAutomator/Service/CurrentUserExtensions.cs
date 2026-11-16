using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EF_InternApplicationAutomator.API.Service
{

    //Token'dan ID, UserID, internID gibi bilgileri okuyan fonksiyon
    public static class CurrentUserExtensions
    {
        public static int? GetPersonID(this ClaimsPrincipal user)
                   => ReadInt(user, ClaimTypes.NameIdentifier);

        public static int? GetUserID(this ClaimsPrincipal user)
            => ReadInt(user, TokenService.UserIdClaim);

        public static int? GetInternID(this ClaimsPrincipal user)
            => ReadInt(user, TokenService.InternIdClaim);

        private static int? ReadInt(ClaimsPrincipal user, string claimType)
        {
            string? value = user.FindFirstValue(claimType);
            return int.TryParse(value, out int id) ? id : null;
        }

        //user'ın rölü staff mı
        public static bool IsStaff(this ClaimsPrincipal user)
           => user.IsInRole("User");

    }
}
