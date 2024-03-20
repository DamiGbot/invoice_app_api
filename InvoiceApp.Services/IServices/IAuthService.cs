using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Requests;
using InvoiceApp.Data.Responses;

namespace InvoiceApp.Services.IServices
{
    public interface IAuthService
    {
        Task<ResponseDto<string>> Register(RegistrationRequest request);
        Task<ResponseDto<AuthResponse>> Login(AuthRequest request);
        Task<ResponseDto<string>> InitiatePasswordReset(InitiatePasswordResetDto request);
        Task<ResponseDto<string>> ConfirmPasswordReset(ConfirmPasswordResetDto request);
        Task<ResponseDto<UserDto>> ConfirmEmail(ConfirmEmailRequestDto request);
        //Task<ResponseDto<string>> ResendConfirmationEmail(EmailRequestDto request, string? loggedInAdminEmail);
        Task<ResponseDto<RefreshTokenDto>> GetRefreshToken(RefreshTokenDto request);
    }
}
