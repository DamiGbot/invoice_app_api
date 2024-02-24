using InvoiceApp.Data.Enums;

namespace InvoiceApp.Data.DTO
{
    public class UserDto
    {
        public string Id { get; set; } 
        public string? Email { get; set; } 
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public string? UserName { get; set; } 
        public UserStatus Status { get; set; }
        //public object? ProfilePicture { get; set; } = null!;
    }
}
