using InvoiceApp.Data.Enums;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;



namespace InvoiceApp.Data.Models
{
    public class Invoice : BaseEntity
    {
        public string UserID { get; set; }
        [JsonIgnore]
        [ForeignKey(nameof(UserID))]
        public virtual ApplicationUser User { get; set; }
        public string FrontendId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PaymentDue { get; set; }
        public string Description { get; set; }
        public int PaymentTerms { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public InvoiceStatus Status { get; set; }
        public string SenderAddressID { get; set; }
        public bool IsRecurring { get; set; } = false;
        public RecurrencePeriod? RecurrencePeriod { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public int RecurrenceCount { get; set; } = 0;

        [ForeignKey(nameof(SenderAddressID))]
        public virtual Address SenderAddress { get; set; }
        public string ClientAddressID { get; set; }
        
        [ForeignKey(nameof(ClientAddressID))]
        public virtual Address ClientAddress { get; set; }
        public decimal Total { get; set; }
        public List<Item> Items { get; set; }
    }

    public enum RecurrencePeriod
    {
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}
