using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceApp.Data.DTO
{
    public class ConfirmPasswordResetDto
    {
        public string Password { get; set; } 
        public string UserName { get; set; } 
        public string Token { get; set; } 
    }
}
