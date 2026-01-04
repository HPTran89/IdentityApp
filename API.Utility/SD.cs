using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Utility
{
    public static class SD
    {
        // Cookie
        public const  string IdentityAppCookie = "identityappcookie";

        // Application Claims
        public const string UserId = "uid";
        public const string UserName = "username";
        public const string Email = "email";

        // Regex
        public const string UserNameRegex = "^[a-zA-Z0-9_.-]*$";

        // Application Rules
        public const int RequiredPasswordLength = 6;
        public const int MaxFailedAccessAttempts = 3;
        public const int DefaultLockoutTimeSpan = 1;
    }
}
