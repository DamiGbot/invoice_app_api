

namespace InvoiceApp.Services.IServices
{
    public interface IInvoiceIdService
    {
        Task<string> GenerateUniqueInvoiceIdForUserAsync(string userId);
        Task RefreshCacheAsync();
    }
}
