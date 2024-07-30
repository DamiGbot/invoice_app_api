


namespace InvoiceApp.Data.Models.IRepository
{
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        Invoice GetInvoiceById(string invoiceId);
        Task<Invoice> GetInvoiceByIdAsync(string invoiceId);
        Task<IEnumerable<Invoice>> GetRecurringInvoicesAsync(DateTime date);
    }
}
