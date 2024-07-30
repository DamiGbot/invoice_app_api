
using System.Linq.Expressions;

namespace InvoiceApp.Data.Models.IRepository
{
    public interface IRecurringInvoiceRepository : IGenericRepository<RecurringInvoice>
    {
        Task<RecurringInvoice> FindAsync(Expression<Func<RecurringInvoice, bool>> predicate);
    }
}
