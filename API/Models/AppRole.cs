using Microsoft.AspNetCore.Identity;

namespace API.Models
{
    public class AppRole : IdentityRole<int>
    {
        // Navigation
        public required ICollection<AppUserRoleBridge> Users { get; set; }
    }
}
