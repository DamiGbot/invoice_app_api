
using InvoiceApp.Data.Models.IRepository;

namespace InvoiceApp.Data.Models.Repository
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        public AddressRepository(InvoiceAppDbContext context) : base(context)
        {
        }
    }
}
