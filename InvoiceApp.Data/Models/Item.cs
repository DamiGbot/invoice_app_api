using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceApp.Data.Models
{
    public class Item
    {
        [Key]
        public int ItemID { get; set; }
        public string InvoiceID { get; set; }
        [ForeignKey(nameof(InvoiceID))]
        public virtual Invoice Invoice { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }
}
