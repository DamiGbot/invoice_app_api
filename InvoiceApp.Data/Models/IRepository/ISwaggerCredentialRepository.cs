
namespace InvoiceApp.Data.Models.IRepository
{
    public interface ISwaggerCredentialRepository : IGenericRepository<SwaggerCredential>
    {
        Task<SwaggerCredential> GetByUsernameAsync(string username);
        Task<List<SwaggerCredential>> GetExpiredSwaggerCredentialsAsync();
    }
}
