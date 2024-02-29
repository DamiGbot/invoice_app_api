
using System.ComponentModel.DataAnnotations;

namespace InvoiceApp.Data.DTO
{
    public class InitiatePasswordResetDto
    {
        [EmailAddress]
        public string Email { get; set; } 
    }
}
