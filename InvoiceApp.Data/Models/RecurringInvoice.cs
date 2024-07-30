using InvoiceApp.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceApp.Data.Models
{
    public class RecurringInvoice : BaseEntity
    {
        public string InvoiceId { get; set; }
        [ForeignKey(nameof(InvoiceId))]
        public Invoice Invoice { get; set; }

        public DateTime RecurrenceDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal Total { get; set; }
    }
}
