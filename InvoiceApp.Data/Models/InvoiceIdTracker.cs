
namespace InvoiceApp.Data.Models
{
    public class InvoiceIdTracker : PrimaryKeyEntity
    {
        public string UserId { get; set; }
        public string FrontendId { get; set; }
    }
}
