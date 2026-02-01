using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Account
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }

        private string _email;
        [Required]
        [EmailAddress(ErrorMessage = "Invaild Email")]
        public string Email { get => _email; set => _email = value.ToLower(); }

        [Required]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Password must be at least {2} and maximum {1} characters")]
        public required string NewPassword { get; set; }

    }
}
