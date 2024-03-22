
namespace InvoiceApp.Data.Models.IRepository
{
    public interface IUnitOfWork
    {
        IApplicationUserRepository ApplicationUserRepository {  get; }
        IInvoiceRepository InvoiceRepository { get; }
        IItemRepository ItemRepository { get; }
        IAddressRepository AddressRepository { get; }
        IInvoiceIdTrackersRepository InvoiceIdTrackersRepository { get; }
        IProfilePictureRepository ProfilePictureRepository { get; }
        ISwaggerCredentialRepository SwaggerCredentialRepository { get; }
        Task BeginTransactionAsync();
        Task SaveAsync(CancellationToken cancellationToken);
        Task CommitAsync();
        Task RollbackAsync();
    }
}
