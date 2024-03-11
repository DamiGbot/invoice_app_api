using InvoiceApp.Data.Enums;

namespace InvoiceApp.Data.DTO
{
    public class InvoiceRequestDto
    {
        public string Description { get; set; }
        public int PaymentTerms { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string Status { get; set; }
        public AddressDto SenderAddress { get; set; }
        public AddressDto ClientAddress { get; set; }
        public List<ItemDto> Items { get; set; }
    }
}
