using InvoiceApp.Data.Enums;
using InvoiceApp.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceApp.Data.DTO
{
    public class InvoiceResponseDto 
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PaymentDue { get; set; }
        public string Description { get; set; }
        public int PaymentTerms { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public InvoiceStatus Status { get; set; }
        public virtual Address SenderAddress { get; set; }
        public virtual Address ClientAddress { get; set; }
        public decimal Total { get; set; }
        public List<ItemDto> Items { get; set; }
    }
}
