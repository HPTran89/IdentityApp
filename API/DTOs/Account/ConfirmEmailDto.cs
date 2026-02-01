using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Account
{
    public class ConfirmEmailDto
    {
        public string Token { get; set; }

        private string _email;
        [Required]
        [EmailAddress(ErrorMessage = "Invaild Email")]
        public string Email { get => _email; set => _email = value.ToLower(); }

    }

    public class EmailDto
    {
            private string _email;
            [Required]
            [EmailAddress(ErrorMessage = "Invaild Email")]
            public string Email { get => _email; set => _email = value.ToLower(); }

    }
}
