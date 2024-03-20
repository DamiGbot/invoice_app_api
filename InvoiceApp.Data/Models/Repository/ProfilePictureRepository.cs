
using InvoiceApp.Data.Models.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApp.Data.Models.Repository
{
    public class ProfilePictureRepository : GenericRepository<ProfilePicture>, IProfilePictureRepository
    {
        public ProfilePictureRepository(InvoiceAppDbContext context) : base(context)
        {
        }

        public async Task<ProfilePicture?> GetByUserIdAsync(string userId)
        {
            var profilePicture = await _context.Set<ProfilePicture>()
                .Where(pp => pp.UserId == userId)
                .FirstOrDefaultAsync();

            return profilePicture;
        }
    }
}
