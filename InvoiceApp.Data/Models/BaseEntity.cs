using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InvoiceApp.Data.Models
{
    public class BaseEntity
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
