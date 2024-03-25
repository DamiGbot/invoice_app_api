using InvoiceApp.Data.DTO;
using InvoiceApp.SD;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace InvoiceApp.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(CustomProblemDetails))]
    public class SwaggerCredentialsController : Controller
    {
        private readonly ISwaggerCredentialsService _swaggerCredentialsService;
        public SwaggerCredentialsController(ISwaggerCredentialsService swaggerCredentialsService)
        {
            _swaggerCredentialsService = swaggerCredentialsService;
        }

        //[HttpPost("credentials")] 
        //[SwaggerOperation(Summary = "Create Swagger Credentials", Description = "Generates new Swagger access credentials with a 24-hour expiry.")]
        //[SwaggerResponse(StatusCodes.Status200OK, "Credentials generated successfully.", typeof(ResponseDto<SwaggerCredentialResponseDto>))]
        //[SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request. Please check the request payload.")]
        //[SwaggerResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")]
        //public async Task<IActionResult> GenerateCredentials([FromBody] SwaggerCredentialRequestDto swaggerCredentialRequestDto)
        //{
        //    var credentials = await _swaggerCredentialsService.GenerateCredentialsAsync(swaggerCredentialRequestDto);
        //    return Ok(credentials);
        //}

        [HttpGet("credentials")]
        [SwaggerOperation(Summary = "Create Swagger Credentials", Description = "Generates new Swagger access credentials with a 24-hour expiry.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Credentials generated successfully.", typeof(ResponseDto<SwaggerCredentialResponseDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")]
        public async Task<IActionResult> GenerateCredentials()
        {
            var credentials = await _swaggerCredentialsService.GenerateCredentialsAsync(null);
            return Ok(credentials);
        }
    }
}
