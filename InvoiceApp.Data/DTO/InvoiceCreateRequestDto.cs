
using InvoiceApp.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace InvoiceApp.Data.DTO
{
    public class InvoiceCreateRequestDto
    {
        public string Description { get; set; }
        public int PaymentTerms { get; set; }
        public string ClientName { get; set; }
        [EmailAddress]
        public string ClientEmail { get; set; }
        public string CreatedAt { get; set; }
        public bool isReady { get; set; }
        //public string Status { get; set; }
        public AddressDto SenderAddress { get; set; }
        public AddressDto ClientAddress { get; set; }
        public List<ItemDto> Items { get; set; }
        public bool IsRecurring { get; set; }
        public RecurrencePeriod? RecurrencePeriod { get; set; }
        public string RecurrenceEndDate { get; set; }

    }
}
