
using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Models;
using InvoiceApp.SD;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

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

        public async Task<ResponseDto<bool>> DeactivateAccountAsync(string userId)
        {
            _logger.LogInformation($"Attempting to deactivate account {userId}.");

            if (!IsUserAuthorized(userId))
            {
                _logger.LogWarning($"Unauthorized attempt to deactivate account {userId}.");
                return new ResponseDto<bool> { IsSuccess = false, Message = "Unauthorized to deactivate this account." };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"DeactivateAccountAsync: User not found for ID {userId}.");
                return new ResponseDto<bool> { IsSuccess = false, Message = "User not found." };
            }

            user.IsDeactivated = true;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"DeactivateAccountAsync Error: {error.Code} - {error.Description}");
                }
                return new ResponseDto<bool> { IsSuccess = false, Message = "Failed to deactivate account." };
            }

            _logger.LogInformation($"User {userId} account deactivated successfully.");
            return new ResponseDto<bool> { IsSuccess = true, Message = "Account deactivated successfully." };
        }


        public async Task<ResponseDto<bool>> DeleteAccountAsync(string userId)
        {
            _logger.LogInformation($"Attempting to delete account {userId}.");

            if (!IsUserAuthorized(userId))
            {
                _logger.LogWarning($"Unauthorized attempt to delete account {userId}.");
                return new ResponseDto<bool> { IsSuccess = false, Message = "Unauthorized to delete this account." };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"DeleteAccountAsync: User not found for ID {userId}.");
                return new ResponseDto<bool> { IsSuccess = false, Message = "User not found." };
            }

            // Schedule deletion after 30 days
            user.ScheduledDeletionDate = DateTime.UtcNow.AddDays(30);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                // Log specific errors from the result for debugging
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"DeleteAccountAsync Error: {error.Code} - {error.Description}");
                }
                return new ResponseDto<bool> { IsSuccess = false, Message = "Failed to schedule account for deletion." };
            }

            _logger.LogInformation($"User {userId} account scheduled for deletion in 30 days.");
            // Update the success message to reflect the 30-day retention period
            return new ResponseDto<bool> { IsSuccess = true, Message = "Your account has been scheduled for deletion and will be permanently removed after 30 days." };
        }


        private bool IsUserAuthorized(string userId)
        {
            var currentUser = _httpContextAccessor.HttpContext.User;
            var currentUserId = _userManager.GetUserId(currentUser);
            var isAdmin = currentUser.IsInRole("Admin");

            // Allow if the user is admin or the current user is performing the action on their own account
            return isAdmin || currentUserId == userId;
        }

    }
}
