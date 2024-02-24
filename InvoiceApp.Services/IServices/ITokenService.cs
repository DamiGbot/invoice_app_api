using InvoiceApp.Data.Models;
using System.IdentityModel.Tokens.Jwt;

namespace InvoiceApp.Services.IServices
{
    public interface ITokenService
    {
        JwtSecurityToken CreateToken(ApplicationUser user);
        string GenerateRefreshToken();
    }
}
