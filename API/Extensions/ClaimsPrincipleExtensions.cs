using API.Utility;
using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaims = user.FindFirst(SD.UserId)?.Value;

            return int.TryParse(userIdClaims, out int userId) ? userId : null;
        }
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(SD.UserName)?.Value;
        }
        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(SD.Email)?.Value;
        }
    }
}
