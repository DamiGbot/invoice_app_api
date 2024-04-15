using InvoiceApp.Data.DTO;
using InvoiceApp.Services.Helper;


namespace InvoiceApp.Services.IServices
{
    public interface IInvoiceService
    {
        Task<ResponseDto<string>> AddInvoiceAsync(InvoiceCreateRequestDto invoiceRequestDto);
        Task<ResponseDto<bool>> DeleteInvoiceAsync(string invoiceId);
        Task<ResponseDto<bool>> EditInvoiceAsync(string invoiceId, InvoiceRequestDto invoiceRequestDto);
        Task<ResponseDto<List<InvoiceResponseDto>>> GetAllInvoicesForUserAsync(string userId);
        Task<ResponseDto<PaginatedList<InvoiceResponseDto>>> GetAllInvoicesForUserAsync(string userId, PaginationParameters paginationParameters);
        Task<ResponseDto<InvoiceResponseDto>> GetInvoiceById(string invoiceId);
        Task<ResponseDto<bool>> MarkInvoiceAsPaidAsync(string invoiceId);
        Task<ResponseDto<bool>> MarkInvoiceAsPendingAsync(string invoiceId);
    }
}
