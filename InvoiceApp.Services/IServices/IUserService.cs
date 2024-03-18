
using InvoiceApp.Data.DTO;

namespace InvoiceApp.Services.IServices
{
    public interface IUserService
    {
        Task<ResponseDto<bool>> UpdateUserDetailsAsync(string userId, UserUpdateDto userDetails);
    }
}
