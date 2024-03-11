using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Models;
using InvoiceApp.Services.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceApp.Services.IServices
{
    public interface IInvoiceService
    {
        Task<ResponseDto<string>> AddInvoiceAsync(InvoiceRequestDto invoiceRequestDto);
        Task<ResponseDto<bool>> DeleteInvoiceAsync(string invoiceId);
        Task<ResponseDto<bool>> EditInvoiceAsync(string invoiceId, InvoiceRequestDto invoiceRequestDto);
        Task<ResponseDto<List<InvoiceResponseDto>>> GetAllInvoicesForUserAsync(string userId);
        Task<ResponseDto<PaginatedList<InvoiceResponseDto>>> GetAllInvoicesForUserAsync(string userId, PaginationParameters paginationParameters);
        Task<ResponseDto<InvoiceResponseDto>> GetInvoiceById(string invoiceId);
    }
}
