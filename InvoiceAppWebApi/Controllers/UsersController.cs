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

        [HttpPut("{userId}/deactivate")]
        [SwaggerOperation(Summary = "Deactivate User Account", Description = "Deactivates a user's account based on the provided user ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Account deactivated successfully.", typeof(ResponseDto<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Could not deactivate account due to a problem with the request.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User account not found.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")]
        public async Task<IActionResult> DeactivateAccount(string userId)
        {
            var response = await _userService.DeactivateAccountAsync(userId);
            if (!response.IsSuccess)
            {
                if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("{userId}")]
        [SwaggerOperation(Summary = "Delete User Account", Description = "Permanently deletes a user's account based on the provided user ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Account deleted successfully.", typeof(ResponseDto<bool>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Could not delete account due to a problem with the request.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User account not found.")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")]
        public async Task<IActionResult> DeleteAccount(string userId)
        {
            var response = await _userService.DeleteAccountAsync(userId);
            if (!response.IsSuccess)
            {
                if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(response);
                }
                return BadRequest(response);
            }
            return Ok(response);
        }

    }
}
