
using System.ComponentModel.DataAnnotations;

namespace InvoiceApp.Data.DTO
{
    public class ItemDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;

    }
}
