using InvoiceApp.Data.Enums;
using Microsoft.AspNetCore.Identity;

namespace InvoiceApp.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Role Role { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}
