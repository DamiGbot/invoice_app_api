
using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Models;
using InvoiceApp.SD;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace InvoiceApp.Services.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ResponseDto<bool>> UpdateUserDetailsAsync(string userId, UserUpdateDto userDetails)
        {
            var userEmail = _httpContextAccessor.HttpContext.Items["Email"];
            var currentUserId = _httpContextAccessor.HttpContext.Items["UserId"];
            var currentUser = _httpContextAccessor.HttpContext.User;
            var isAdmin = currentUser.IsInRole("Admin");
            _logger.LogInformation("Attempting to Update {User} deatils at {time}", userEmail, Contants.currDateTime);

            if (!isAdmin && currentUserId != userId)
            {
                return new ResponseDto<bool> { IsSuccess = false, Message = "Unauthorized to update these details." };
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found.");
                    return new ResponseDto<bool> { IsSuccess = false, Message = "User not found." };
                }

                // Map the DTO to the user entity and save changes
                user.FirstName = !string.IsNullOrEmpty(userDetails.FirstName) ? userDetails.FirstName : user.FirstName;
                user.LastName = !string.IsNullOrEmpty(userDetails.LastName) ? userDetails.LastName : user.LastName;
                user.Email = !string.IsNullOrEmpty(userDetails.Email) ? userDetails.Email : user.Email;
                user.PhoneNumber = userDetails.PhoneNumber ?? user.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {userId} updated successfully.");
                    return new ResponseDto<bool> { IsSuccess = true, Message = "User details updated successfully." };
                }
                else
                {
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Failed to update user details." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user {userId}.");
                return new ResponseDto<bool> { IsSuccess = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
    }
}
