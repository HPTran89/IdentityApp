using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    /// <summary>
    /// For creating a random code for email confirmation
    /// </summary>
    public class AppUserToken: IdentityUserToken<int>
    {
        [Required]
        public DateTime Expires { get; set; }

        // Navigations
        public AppUser User { get; set; }
    }
}
