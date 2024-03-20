using InvoiceApp.Data.Models.IRepository;
using Microsoft.EntityFrameworkCore.Storage;

namespace InvoiceApp.Data.Models.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private IDbContextTransaction _transaction;
        public InvoiceAppDbContext _context;

        public UnitOfWork(InvoiceAppDbContext context) => _context = context;

        #region Dispose
        private bool _disposedValue = false;
        ~UnitOfWork() => Dispose(disposing: false);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing && _context != null)
                {
                    _context.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Repositories
        public IInvoiceRepository InvoiceRepository => new InvoiceRepository(_context);
        public IItemRepository ItemRepository => new ItemRepository(_context);
        public IAddressRepository AddressRepository => new AddressRepository(_context);
        public IInvoiceIdTrackersRepository InvoiceIdTrackersRepository => new InvoiceIdTrackersRepository(_context);
        public IApplicationUserRepository ApplicationUserRepository => new ApplicationUserRepository(_context);
        public IProfilePictureRepository ProfilePictureRepository => new ProfilePictureRepository(_context);
        #endregion Repositories

        public async Task BeginTransactionAsync() => this._transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false);

        public async Task CommitAsync()
        {
            await this._transaction.CommitAsync().ConfigureAwait(false);
            Dispose();
        }

        public async Task RollbackAsync()
        {
            await this._transaction.RollbackAsync().ConfigureAwait(false);
            Dispose();
        }

        public async Task SaveAsync(CancellationToken cancellationToken) =>
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
