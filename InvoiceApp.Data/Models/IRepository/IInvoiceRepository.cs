

namespace InvoiceApp.Data.Models.IRepository
{
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        Task<Invoice> GetInvoiceByIdAsync(string invoiceId);
    }
}
