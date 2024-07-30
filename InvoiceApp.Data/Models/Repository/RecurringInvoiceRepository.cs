using InvoiceApp.Data.Models.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace InvoiceApp.Data.Models.Repository
{
    public class RecurringInvoiceRepository : GenericRepository<RecurringInvoice>, IRecurringInvoiceRepository
    {
        public RecurringInvoiceRepository(InvoiceAppDbContext context) : base(context)
        {
        }

        public async Task<RecurringInvoice> FindAsync(Expression<Func<RecurringInvoice, bool>> predicate)
        {
            return await _context.RecurringInvoices.FirstOrDefaultAsync(predicate);
        }
    }
}
