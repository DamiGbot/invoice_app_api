using InvoiceApp.Data.Models.IRepository;

namespace InvoiceApp.Data.Models.Repository
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(InvoiceAppDbContext context) : base(context)
        {
        }

    }
}
