using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InvoiceApp.Data.Models
{
    public class BaseEntity : PrimaryKeyEntity
    {
        [JsonIgnore]
        public string? Created_by { get; set; }
        [JsonIgnore]
        public DateTime? Created_at { get; set; }
        [JsonIgnore]
        public string? Updated_by { get; set; }
        [JsonIgnore]
        public DateTime? Updated_at { get; set; }
    }
}
