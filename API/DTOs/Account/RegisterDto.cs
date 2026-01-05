using API.Utility;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Account
{
    public class RegisterDto
    {
        [Required]
        [StringLength(15, MinimumLength =3, ErrorMessage ="Name (username) must be at least {2} and maximum {1} characters")]
        [RegularExpression(SD.UserNameRegex, ErrorMessage = "Name (username) must start with a letter and can contain only letters (a-z, A-Z) and numbers (0-9)")]
        public string Name { get; set; }

        private string _email;
        [Required]
        [EmailAddress(ErrorMessage = "Invaild Email")]
        public string Email { get => _email; set => _email = value.ToLower(); }

        [Required]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Username must be at least {2} and maximum {1} characters")]
        public required string Password { get; set; }
    }
}
