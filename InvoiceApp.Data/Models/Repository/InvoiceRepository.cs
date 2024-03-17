using InvoiceApp.Data.Models.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApp.Data.Models.Repository
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(InvoiceAppDbContext context) : base(context)
        {
        }

        public async Task<Invoice> GetInvoiceByIdAsync(string invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.SenderAddress)
                .Include(i => i.ClientAddress)
                .Include(i => i.Items)
                .SingleOrDefaultAsync(i => i.Id == invoiceId);
        }

    }
}
