using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Enums;
using InvoiceApp.Data.Models;
using InvoiceApp.Data.Models.Repository;
using InvoiceApp.Data.Requests;
using InvoiceApp.Data.Responses;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace InvoiceAppApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(CustomProblemDetails))]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly InvoiceAppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(UserManager<ApplicationUser> userManager, InvoiceAppDbContext context, ITokenService tokenService, ILogger<AuthController> logger, IAuthService authService)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
            _authService = authService;
        }

        [HttpPost]
        [Route("register")]
        [SwaggerResponse(StatusCodes.Status201Created, "Request Successful", typeof(ResponseDto<string>))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Request Unsuccessful", typeof(ResponseDto<string>))]
        public async Task<IActionResult> Register(RegistrationRequestDTO registrationRequestDto)
        {
            _logger.LogInformation("Processing register request {@RegistrationRequestDTO}", registrationRequestDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register - Validation errors in user registration for {0}", registrationRequestDto
                    .Email);
                return BadRequest(ModelState);
            }

            RegistrationRequest request = new()
            {
                Email = registrationRequestDto.Email,
                Username = registrationRequestDto.Username.ToLower(),
                Password = registrationRequestDto.Password,
                FirstName = registrationRequestDto.FirstName.ToLower(),
                LastName = registrationRequestDto.LastName.ToLower(),
                Role = Role.User
            };
            var response = await _authService.Register(request);
            if (response.IsSuccess)
            {
                return Created(response.Result, response);
            }

            return Conflict(response);
        }

        [HttpPost]
        [Route("login")]
        [SwaggerOperation(Summary = "Login for all users")]
        [SwaggerResponse(StatusCodes.Status200OK, "Request Successful", typeof(ResponseDto<AuthResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Request Unsuccessful", typeof(ResponseDto<string>))]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ResponseDto<AuthResponse> response = await _authService.Login(request);
            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return Unauthorized(response);
        }

        [HttpPost("initiate-reset-password")]
        [SwaggerOperation(Summary = "Initiate reset password for all users")]
        [SwaggerResponse(StatusCodes.Status200OK, "Request Processed", typeof(ResponseDto<string>))]
        public async Task<IActionResult> InitiateResetPassword([FromBody] InitiatePasswordResetDto model)
        {
            ResponseDto<string> response = await _authService.InitiatePasswordReset(model);
            return Ok(response);
        }

        [HttpPost("confirm-reset-password")]
        [SwaggerOperation(Summary = "Confirm reset password for all users")]
        [SwaggerResponse(StatusCodes.Status200OK, "Request Processed", typeof(ResponseDto<string>))]
        public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmPasswordResetDto model)
        {
            ResponseDto<string> response = await _authService.ConfirmPasswordReset(model);
            return Ok(response);
        }

        [HttpPost("confirm-email")]
        [SwaggerOperation(Summary = "Confirm email for all users")]
        [SwaggerResponse(StatusCodes.Status200OK, "Request Processed", typeof(ResponseDto<ConfirmEmailResponseDto>))]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDto model)
        {
            ResponseDto<UserDto> response = await _authService.ConfirmEmail(model);
            return Ok(response);
        }

        //[HttpPost("refresh-token")]
        //[SwaggerOperation(Summary = "Refresh Token")]
        //[SwaggerResponse(StatusCodes.Status200OK, "Request Processed", typeof(ResponseDto<AuthResponseDto>))]
        //public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        //{
        //    ResponseDto<RefreshTokenDto> response = await this._authService.GetRefreshToken(model);
        //    return Ok(response);
        //}
    }
}
