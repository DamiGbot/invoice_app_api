using Microsoft.AspNetCore.Mvc;

namespace InvoiceAppApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]

    public class InvoiceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
