
using InvoiceApp.Data.DTO;

namespace InvoiceApp.Data.Responses
{
    public class AuthResponse
    {
        public UserDto? User { get; set; }
        public string AccessToken { get; set; } 
        public string RefreshToken { get; set; }
        public IList<string> Role { get; set; } = default!;
    }
}
