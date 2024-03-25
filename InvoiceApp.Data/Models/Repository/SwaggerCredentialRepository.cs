
using InvoiceApp.Data.Models.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceApp.Data.Models.Repository
{
    public class SwaggerCredentialRepository : GenericRepository<SwaggerCredential>, ISwaggerCredentialRepository
    {
        private readonly ILogger<SwaggerCredentialRepository> _logger;
        public SwaggerCredentialRepository(InvoiceAppDbContext context, ILogger<SwaggerCredentialRepository> logger) : base(context)
        {
            _logger = logger;
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

            if (expiredCredentials.Any())
            {
                _context.SwaggerCredentials.RemoveRange(expiredCredentials);
                _logger.LogInformation($"{expiredCredentials.Count} expired Swagger credentials cleared successfully.");
            }
            else
            {
                _logger.LogInformation("No expired Swagger credentials found to clear.");
            }
        }
    }
}
