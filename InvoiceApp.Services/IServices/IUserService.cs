
using InvoiceApp.Data.DTO;

namespace InvoiceApp.Services.IServices
{
    public interface IUserService
    {
        Task<ResponseDto<UserDto>> GetUserDetailsAsync(string userId);
        Task<ResponseDto<bool>> UpdateUserDetailsAsync(string userId, UserUpdateDto userDetails);
        Task<ResponseDto<bool>> DeactivateAccountAsync(string userId);
        Task<ResponseDto<bool>> DeleteAccountAsync(string userId);
        Task<ResponseDto<bool>> HardDeleteAccountAsync(string userId);
        Task<ResponseDto<bool>> ActivateAccountAsync(string userId);
    }
}
