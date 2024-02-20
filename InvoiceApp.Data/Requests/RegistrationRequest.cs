using InvoiceApp.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace InvoiceApp.Data.Requests
{
    public class RegistrationRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        public Role Role { get; set; }
    }
}
