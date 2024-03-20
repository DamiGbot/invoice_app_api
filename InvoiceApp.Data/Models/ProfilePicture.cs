using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InvoiceApp.Data.Models
{
    public class ProfilePicture : BaseEntity
    {
        public string ImageData { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string ContentType { get; set; }
        public string UserId { get; set; }

        // Navigation Property
        [JsonIgnore]
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        private ProfilePicture(
            string imageData,
            string name,
            string contentType,
            string userId
        )
        {
            ImageData = imageData;
            Name = name;
            ContentType = contentType;
            UserId = userId;
        }

        public static ProfilePicture Create(
            string imageData,
            string name,
            string contentType,
            string userId) =>
            new(imageData, name, contentType, userId);
    }
}
