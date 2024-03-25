using InvoiceApp.Data.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceApp.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public Role Role { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public bool IsLockedOutByAdmin { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? AddressID { get; set; }
        [ForeignKey(nameof(AddressID))]
        public virtual Address? Address { get; set; }
        public virtual ProfilePicture? ProfilePicture { get; set; } = null!;
        public virtual ICollection<Invoice> Invoices { get; set; }
        public bool IsDeactivated { get; set; } = false;
        public DateTime? ScheduledDeletionDate { get; set; }
    }
}
