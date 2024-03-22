
namespace InvoiceApp.Data.Models
{
    public class SwaggerCredential : PrimaryKeyEntity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
