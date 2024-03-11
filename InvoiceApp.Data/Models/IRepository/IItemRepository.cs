

namespace InvoiceApp.Data.Models.IRepository
{
    public interface IItemRepository : IGenericRepository<Item>
    {
        Task SaveListAsync(IEnumerable<Item> items);
    }
}
