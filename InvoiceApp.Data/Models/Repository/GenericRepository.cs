using InvoiceApp.Data.Models.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApp.Data.Models.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected internal readonly InvoiceAppDbContext _context;

        public GenericRepository(InvoiceAppDbContext context)
        {
            _context = context;
        }
        public async Task<T> GetByIdAsync(object id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
