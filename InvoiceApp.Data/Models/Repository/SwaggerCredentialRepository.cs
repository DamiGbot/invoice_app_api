
using InvoiceApp.Data.Models.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApp.Data.Models.Repository
{
    public class SwaggerCredentialRepository : GenericRepository<SwaggerCredential>, ISwaggerCredentialRepository
    {
        public SwaggerCredentialRepository(InvoiceAppDbContext context) : base(context)
        {
        }

        public async Task<SwaggerCredential> GetByUsernameAsync(string username)
        {
            return await _context.SwaggerCredentials
                                 .FirstOrDefaultAsync(cred => cred.Username == username);
        }

        public async Task<List<SwaggerCredential>> GetExpiredSwaggerCredentialsAsync()
        {
            var expiredCredentials = await _context.SwaggerCredentials
                                                   .Where(cred => cred.ExpiryTime <= DateTime.UtcNow)
                                                   .ToListAsync();
            return expiredCredentials;
        }

        public async Task ClearExpiredSwaggerCredentialsAsync()
        {
            var expiredCredentials = await GetExpiredSwaggerCredentialsAsync();
            _context.SwaggerCredentials.RemoveRange(expiredCredentials);
        }
    }
}
