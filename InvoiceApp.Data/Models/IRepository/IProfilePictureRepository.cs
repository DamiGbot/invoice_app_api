
namespace InvoiceApp.Data.Models.IRepository
{
    public interface IProfilePictureRepository : IGenericRepository<ProfilePicture>
    {
        Task<ProfilePicture?> GetByUserIdAsync(string userId);
    }
}
