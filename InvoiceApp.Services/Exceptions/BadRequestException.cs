using System.ComponentModel.DataAnnotations;

namespace InvoiceApp.Services.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {

        }

        public IDictionary<string, string[]>? ValidationErrors { get; set; }
    }
}
