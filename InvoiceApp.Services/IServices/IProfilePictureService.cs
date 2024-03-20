
using InvoiceApp.Data.DTO;

namespace InvoiceApp.Services.IServices
{
    public interface IProfilePictureService
    {
        Task<ResponseDto<string>> UploadProfilePicture(ProfilePictureUploadRequestDto request,
            CancellationToken cancellationToken);
    }
}
