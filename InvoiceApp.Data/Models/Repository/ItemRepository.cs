
using InvoiceApp.Data.Models.IRepository;

namespace InvoiceApp.Data.Models.Repository
{
    public class ItemRepository : GenericRepository<Item>, IItemRepository
    {
        public ItemRepository(InvoiceAppDbContext context) : base(context)
        {
        }

        public async Task SaveListAsync(IEnumerable<Item> items)
        {
            await _context.Set<Item>().AddRangeAsync(items);
            await _context.SaveChangesAsync();
        }
    }
}
