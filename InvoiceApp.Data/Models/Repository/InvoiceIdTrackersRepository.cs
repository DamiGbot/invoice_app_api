
using InvoiceApp.Data.Models.IRepository;

namespace InvoiceApp.Data.Models.Repository
{
    public class InvoiceIdTrackersRepository : GenericRepository<InvoiceIdTracker>, IInvoiceIdTrackersRepository
    {
        public InvoiceIdTrackersRepository(InvoiceAppDbContext context) : base(context)
        {
        }
    }
}
