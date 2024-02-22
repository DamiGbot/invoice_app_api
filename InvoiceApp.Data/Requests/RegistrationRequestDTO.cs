using System.ComponentModel.DataAnnotations;

namespace InvoiceApp.Data.Requests
{
    public class RegistrationRequestDTO
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
