
using System.ComponentModel.DataAnnotations;

namespace InvoiceApp.Data.Models
{
    public class PrimaryKeyEntity
    {
        public PrimaryKeyEntity()
        {
            Id = Guid.NewGuid().ToString();
        }

        [Key]
        public string Id { get; set; }
    }
}
