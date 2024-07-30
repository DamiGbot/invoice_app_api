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

        public Invoice GetInvoiceById(string invoiceId)
        {
            return _context.Invoices.FirstOrDefault(i => i.Id.Equals(invoiceId));
        }

        public async Task<IEnumerable<Invoice>> GetRecurringInvoicesAsync(DateTime date)
        {
            var recurringInvoices = await _context.Invoices
                .Where(i => i.IsRecurring && i.RecurrenceEndDate >= date)
                .ToListAsync();

            return recurringInvoices.Where(i => ShouldGenerateInvoice(i, date));
        }

        private static bool ShouldGenerateInvoice(Invoice invoice, DateTime date)
        {
            return invoice.RecurrencePeriod switch
            {
                RecurrencePeriod.Daily => true,
                RecurrencePeriod.Weekly => (date - invoice.CreatedAt).TotalDays % 7 == 0,
                RecurrencePeriod.Monthly => invoice.CreatedAt.Day == date.Day,
                RecurrencePeriod.Yearly => invoice.CreatedAt.Month == date.Month && invoice.CreatedAt.Day == date.Day,
                _ => false
            };
        }


    }
}
