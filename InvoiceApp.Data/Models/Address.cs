using System.ComponentModel.DataAnnotations;

namespace InvoiceApp.Data.Models
{
    public class Address
    {
        [Key]
        public int AddressID { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
    }
}
