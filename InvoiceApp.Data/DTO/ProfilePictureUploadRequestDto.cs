using Microsoft.AspNetCore.Http;

namespace InvoiceApp.Data.DTO
{
    public class ProfilePictureUploadRequestDto
    {
        public IFormFile? File { get; set; } = null!;
        public string LastName { get; set; } = default!;
        public string FirstName { get; set; } = default!;
    }
}
