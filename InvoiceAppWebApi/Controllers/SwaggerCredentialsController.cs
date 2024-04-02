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

        //[HttpGet("credentials")]
        //[SwaggerOperation(Summary = "Create Swagger Credentials", Description = "Generates new Swagger access credentials with a 24-hour expiry.")]
        //[SwaggerResponse(StatusCodes.Status200OK, "Credentials generated successfully.", typeof(ResponseDto<SwaggerCredentialResponseDto>))]
        //[SwaggerResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")]
        //public async Task<IActionResult> GenerateCredentials()
        //{
        //    var credentials = await _swaggerCredentialsService.GenerateCredentialsAsync(null);
        //    return Ok(credentials);
        //}

        [HttpGet("credentials")]
        [SwaggerOperation(Summary = "Create Swagger Credentials", Description = "Generates new Swagger access credentials with a 24-hour expiry.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Credentials generated successfully.", typeof(string))] // Note the change here to indicate the response type is a string.
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")]
        public async Task<IActionResult> GenerateCredentials()
        {
            var credentialsResponse = await _swaggerCredentialsService.GenerateCredentialsAsync(null);
            if (!credentialsResponse.IsSuccess)
            {
                // Return an appropriate error response
                return StatusCode(StatusCodes.Status500InternalServerError, credentialsResponse.Message);
            }

            // Create a plain text response containing the credentials
            var credentialsText = $"Username: {credentialsResponse.Result.UserName}\nPassword: {credentialsResponse.Result.Password}\nExpiryTime: {credentialsResponse.Result.ExpiryTime.ToString("yyyy-MM-dd HH:mm:ss")}\nNote: Swagger Credential successfully created and will last for 24 hours. Please note the password is shown only once and cannot be retrieved again.";
            return Content(credentialsText, "text/plain");
        }

    }
}
