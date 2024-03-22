
using InvoiceApp.Data.DTO;

namespace InvoiceApp.Services.IServices
{
    public interface ISwaggerCredentialsService
    {
        Task<ResponseDto<SwaggerCredentialResponseDto>> GenerateCredentialsAsync(SwaggerCredentialRequestDto requestDto);
        Task<bool> ValidateCredentialsAsync(string username, string password);
    }
}
