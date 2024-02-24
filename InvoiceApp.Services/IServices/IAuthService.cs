using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Requests;
using InvoiceApp.Data.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceApp.Services.IServices
{
    public interface IAuthService
    {
        Task<ResponseDto<string>> Register(RegistrationRequest request);
        Task<ResponseDto<AuthResponse>> Login(AuthRequest request);
        //Task<ResponseDto<string>> InitiatePasswordReset(InitiatePasswordResetDto request);
        //Task<ResponseDto<string>> ConfirmPasswordReset(ConfirmPasswordResetDto request);
        //Task<ResponseDto<ConfirmEmailResponseDto>> ConfirmEmail(ConfirmEmailRequestDto request);
        //Task<ResponseDto<string>> ResendConfirmationEmail(EmailRequestDto request, string? loggedInAdminEmail);
        //Task<ResponseDto<RefreshTokenDto>> GetRefreshToken(RefreshTokenDto request);
    }
}
