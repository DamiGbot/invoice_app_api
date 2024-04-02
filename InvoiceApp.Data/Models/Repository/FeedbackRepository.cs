using InvoiceApp.Data.Models.IRepository;

namespace InvoiceApp.Data.Models.Repository
{
    public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(InvoiceAppDbContext context) : base(context)
        {
        }
    }
}
