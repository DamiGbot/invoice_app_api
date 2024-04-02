using InvoiceApp.Data.Enums;
using InvoiceApp.Data.Models;

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
        public bool IsLockedOutByAdmin { get; set; }
        public bool IsDeactivated { get; set; } 
        public ProfilePicture ProfilePicture { get; set; }
        public Address Address { get; set; }
        //public virtual ProfilePicture? ProfilePicture { get; set; }
        //public object? ProfilePicture { get; set; } = null!;
    }
}
