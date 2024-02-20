using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceApp.Data.Responses
{
    public class AuthResponse
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
