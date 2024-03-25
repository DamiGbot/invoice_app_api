
using InvoiceApp.Data.DTO;

namespace InvoiceApp.Services.IServices
{
    public interface IUserService
    {
        Task<ResponseDto<bool>> UpdateUserDetailsAsync(string userId, UserUpdateDto userDetails);
        Task<ResponseDto<bool>> DeactivateAccountAsync(string userId);
        Task<ResponseDto<bool>> DeleteAccountAsync(string userId);
    }
}
