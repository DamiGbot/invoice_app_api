using InvoiceApp.Data.DTO;
using InvoiceApp.SD;
using InvoiceApp.Services.Helper;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace InvoiceAppApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(CustomProblemDetails))]

    public class InvoiceController : Controller
    {

        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost("create")]
        [Authorize]
        [SwaggerOperation(Summary = "Create Invoice")]
        [SwaggerResponse(StatusCodes.Status201Created, "Request Successful", typeof(ResponseDto<string>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(InvoiceRequestDto invoiceRequestDTO)
        {
            var response = await _invoiceService.AddInvoiceAsync(invoiceRequestDTO);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get Invoice by Invoice id")]
        [SwaggerResponse(StatusCodes.Status200OK, "Request Successful", typeof(ResponseDto<InvoiceResponseDto>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetInvoiceById(string id)
        {
            var response = await _invoiceService.GetInvoiceById(id);
            return Ok(response);
        }

        [HttpGet("get-all-invoice/{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Get All invoice for a User")]
        [SwaggerResponse(StatusCodes.Status200OK, "Request Successful", typeof(ResponseDto<List<InvoiceResponseDto>>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Unauthorized to Perform this action")]
        public async Task<IActionResult> GetAllInvoiceForUserInternal(string id)
        {
            var response = await _invoiceService.GetAllInvoicesForUserAsync(id);
            return Ok(response);
        }

        [HttpGet("get-all-invoice")]
        [Authorize]
        [SwaggerOperation(Summary = "Get All invoice for a User")]
        [SwaggerResponse(StatusCodes.Status200OK, "Request Successful", typeof(ResponseDto<List<InvoiceResponseDto>>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Unauthorized to Perform this action")]
        public async Task<IActionResult> GetAllInvoiceForUser()
        {
            var response = await _invoiceService.GetAllInvoicesForUserAsync(null);
            return Ok(response);
        }

        [HttpGet("get-all-invoice-paginatation")]
        [Authorize]
        [SwaggerOperation(Summary = "Get All Invoices for a User with Pagination")]
        [SwaggerResponse(StatusCodes.Status200OK, "Request Successful", typeof(ResponseDto<PaginatedList<InvoiceResponseDto>>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Unauthorized to Perform this action")]
        public async Task<IActionResult> GetAllInvoicesForUser([FromQuery] PaginationParameters paginationParameters)
        {
            var response = await _invoiceService.GetAllInvoicesForUserAsync(null, paginationParameters);
            return Ok(response);
        }

        [HttpPut("edit/{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Edit Invoice")]
        [SwaggerResponse(StatusCodes.Status200OK, "Invoice edited successfully", typeof(ResponseDto<bool>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Cannot edit invoices with status 'Paid'")]
        public async Task<IActionResult> Edit(string id, InvoiceRequestDto invoiceRequestDto)
        {
            var response = await _invoiceService.EditInvoiceAsync(id, invoiceRequestDto);

            if (!response.IsSuccess)
            {
                if (response.Message == "Paid invoices cannot be edited." || response.Message == "Invoice not found.")
                {
                    return BadRequest(response);
                }

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }

            return Ok(response);
        }


        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete Invoice by ID")]
        [SwaggerResponse(StatusCodes.Status200OK, "Invoice deleted successfully", typeof(ResponseDto<bool>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Invoice not found")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Unauthorized to Perform this action")]
        public async Task<IActionResult> DeleteInvoice(string id)
        {
            var response = await _invoiceService.DeleteInvoiceAsync(id);
            if (!response.IsSuccess && response.Message == "Invoice not found")
            {
                return NotFound(response);
            }
            else if (!response.IsSuccess && response.Message == "Unauthorized to delete this invoice")
            {
                return StatusCode(StatusCodes.Status403Forbidden, response);
            }
            return Ok(response);
        }

        [HttpPost("{invoiceId}/mark-as-paid")]
        [Authorize]
        [SwaggerOperation(Summary = "Mark an Invoice as Paid")]
        [SwaggerResponse(StatusCodes.Status200OK, "Invoice marked as paid successfully", typeof(ResponseDto<bool>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Invoice not found")]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MarkInvoiceAsPaid(string invoiceId)
        {
            var response = await _invoiceService.MarkInvoiceAsPaidAsync(invoiceId);
            if (!response.IsSuccess)
            {
                return NotFound(response);
            }
            return Ok(response);
        }


    }
}
