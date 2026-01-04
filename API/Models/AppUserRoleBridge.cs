using Microsoft.AspNetCore.Identity;

namespace API.Models
{
    public class AppUserRoleBridge : IdentityUserRole<int>
    {
        // Navigation
        public required AppUser User { get; set; }
        public required AppRole Role { get; set; }

    }
}
