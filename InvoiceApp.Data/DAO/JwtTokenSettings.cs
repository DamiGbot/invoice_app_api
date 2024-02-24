using System;
namespace InvoiceApp.Data.DAO
{
    public class JwtTokenSettings
    {
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }
        public string SymmetricSecurityKey { get; set; } 
        public string JwtRegisteredClaimNamesSub { get; set; }
        public double DurationInMinutes { get; set; }
        public int RefreshTokenValidityInDays { get; set; }
    }
}
