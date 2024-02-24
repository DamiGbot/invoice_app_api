using InvoiceApp.Data.DAO;
using InvoiceApp.Data.Models;
using InvoiceApp.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InvoiceApp.Services.Services
{
    public class TokenService : ITokenService
    {
        // Specify how long until the token expires
        private const int ExpirationMinutes = 30;
        private readonly ILogger<TokenService> _logger;
        private readonly IConfiguration _config;
        private readonly JwtTokenSettings _jwtTokenSettings;

        public TokenService(ILogger<TokenService> logger, IConfiguration config, JwtTokenSettings jwtTokenSettings)
        {
            _logger = logger;
            _config = config;
            _jwtTokenSettings = jwtTokenSettings;
        }

        public JwtSecurityToken CreateToken(ApplicationUser user)
        {
            var expiration = DateTime.UtcNow.AddMinutes(_jwtTokenSettings.DurationInMinutes);
            var token = CreateJwtToken(
                CreateClaims(user),
                CreateSigningCredentials(),
                expiration
            );
            //var tokenHandler = new JwtSecurityTokenHandler();

            _logger.LogInformation("JWT Token created");

            return token;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
            DateTime expiration) =>
            new(
                issuer: _jwtTokenSettings.ValidIssuer,
                audience: _jwtTokenSettings.ValidAudience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

        private List<Claim> CreateClaims(ApplicationUser user)
        {
            var jwtSub = _jwtTokenSettings.JwtRegisteredClaimNamesSub;

            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, jwtSub),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };

                return claims;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private SigningCredentials CreateSigningCredentials()
        {
            var symmetricSecurityKey = _jwtTokenSettings.SymmetricSecurityKey;

            return new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(symmetricSecurityKey)
                ),
                SecurityAlgorithms.HmacSha256
            );
        }
    }
}
