
namespace InvoiceApp.Data.DTO
{
    public class ConfirmEmailResponseDto
    {
        public UserDto? User { get; set; }
        public string Role { get; set; } 
        public string PasswordResetToken { get; set; } 
    }
}
