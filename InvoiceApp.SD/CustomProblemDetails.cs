using Microsoft.AspNetCore.Mvc;

namespace InvoiceApp.SD
{
    public class CustomProblemDetails : ProblemDetails
    {
        public IDictionary<string, string[]>? Errors { get; set; } = new Dictionary<string, string[]>();
    }
}
