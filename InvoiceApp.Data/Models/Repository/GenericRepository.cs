using InvoiceApp.Data.Models.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return await query.ToListAsync();
        }

    }
}
