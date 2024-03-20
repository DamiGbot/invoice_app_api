
namespace InvoiceApp.Data.DTO
{
    public class AuthResponseDto
    {
        public UserDto? User { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public IList<string> Role { get; set; } = default!;
    }
}
