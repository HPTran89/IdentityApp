using Microsoft.AspNetCore.Identity;

namespace API.Models
{
    public class AppUser : IdentityUser<int>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required string Name { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public  ICollection<AppUserRoleBridge> Roles { get; set; }
        public ICollection<AppUserToken> Tokens { get; set; }

    }
}
