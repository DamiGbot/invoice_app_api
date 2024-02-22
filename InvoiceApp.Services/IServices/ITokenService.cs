using InvoiceApp.Data.Models;

namespace InvoiceApp.Services.IServices
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
    }
}
