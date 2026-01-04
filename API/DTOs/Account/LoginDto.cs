using API.Utility;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Account
{
    public class LoginDto
    {
        private string _username;
        [Required]
        public string Username { get => _username; set => _username = value.ToLower(); }
        [Required]
        public string Password { get; set; }
    }
}
