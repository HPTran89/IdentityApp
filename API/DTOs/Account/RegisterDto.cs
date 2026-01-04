using API.Utility;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Account
{
    public class RegisterDto
    {
        private string _username;
        [Required]
        [StringLength(15, MinimumLength =3, ErrorMessage ="Username must be at least {2} and maximum {1} characters")]
        [RegularExpression(SD.UserNameRegex, ErrorMessage ="Username must contain only a-z A-Z 0-9 characters")]
        public string Username { get => _username; set => _username = value.ToLower(); }

        private string _email;
        [Required]
        [EmailAddress(ErrorMessage = "Invaild Email")]
        public string Email { get => _email; set => _email = value.ToLower(); }

        [Required]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Username must be at least {2} and maximum {1} characters")]
        public required string Password { get; set; }
    }
}
