
namespace InvoiceApp.Data.Models.IRepository
{
    public interface IUnitOfWork
    {
        IInvoiceRepository InvoiceRepository { get; }
        IItemRepository ItemRepository { get; }
        IAddressRepository AddressRepository { get; }
        IInvoiceIdTrackersRepository invoiceIdTrackersRepository { get; }
        Task BeginTransactionAsync();
        Task SaveAsync(CancellationToken cancellationToken);
        Task CommitAsync();
        Task RollbackAsync();
    }
}
