using InvoiceApp.Data.Models;

namespace InvoiceApp.Services
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
    }
}
