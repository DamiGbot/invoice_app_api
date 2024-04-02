namespace InvoiceApp.Data.Models
{
    public class Feedback : BaseEntity
    {
        public string Category { get; set; }
        public string Description { get; set; }
        public string AdditionalInfo { get; set; }
        public string Status { get; set; } = "Active"; 
        public string SubmittedBy { get; set; } // Could be UserID or Email
        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
        public string? AdminComments { get; set; }
    }
}
