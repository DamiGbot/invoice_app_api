using InvoiceApp.Data.DTO;
using InvoiceApp.SD;
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
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPut("{userId}/details")]
        [Authorize]
        [SwaggerOperation(Summary = "Update user details")]
        [SwaggerResponse(StatusCodes.Status200OK, "Request Successful", typeof(ResponseDto<bool>))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Unauthorized to Perform this action")]
        public async Task<IActionResult> UpdateUserDetails(string userId, UserUpdateDto userUpdateDto)
        {
            var response = await _userService.UpdateUserDetailsAsync(userId, userUpdateDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }       
}
