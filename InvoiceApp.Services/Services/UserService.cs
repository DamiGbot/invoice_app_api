
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
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _signInManager = signInManager;
        }

        public async Task<ResponseDto<UserDto>> GetUserDetailsAsync(string userId)
        {
            var requesterId = _httpContextAccessor.HttpContext.Items["UserId"];
            try
            {
                // Authorization check: Ensure the requester is the user or an admin.
                //if (userId != requesterId && !await _userManager.IsInRoleAsync(requesterId, "Admin"))
                if (!IsUserAuthorized(userId))
                {
                    _logger.LogWarning("Unauthorized access attempt to user details by {requesterId}", requesterId);
                    return new ResponseDto<UserDto> { IsSuccess = false, Message = "Unauthorized access." };
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {userId}", userId);
                    return new ResponseDto<UserDto> { IsSuccess = false, Message = "User not found." };
                }

                var userDetails = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.Email,
                    LastName = user.FirstName,
                    UserName = user.UserName,
                    ProfilePicture = user.ProfilePicture,
                    Address = user.Address
                };

                return new ResponseDto<UserDto> { IsSuccess = true, Result = userDetails };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user details for {userId}", userId);
                return new ResponseDto<UserDto> { IsSuccess = false, Message = "An error occurred." };
            }
        }


        public async Task<ResponseDto<bool>> UpdateUserDetailsAsync(string userId, UserUpdateDto userDetails)
        {
            var userEmail = _httpContextAccessor.HttpContext.Items["Email"];
            _logger.LogInformation("Attempting to Update {User} deatils at {time}", userEmail, Contants.currDateTime);

            if (!IsUserAuthorized(userId))
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

            try
            {
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

                SignOutUser(userId);

                _logger.LogInformation($"User {userId} account deactivated successfully.");
                return new ResponseDto<bool> { IsSuccess = true, Message = "Account deactivated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user {userId}.");
                return new ResponseDto<bool> { IsSuccess = false, Message = $"An error occurred: {ex.Message}" };
            }
        }


        public async Task<ResponseDto<bool>> DeleteAccountAsync(string userId)
        {
            _logger.LogInformation($"Attempting to delete account {userId}.");

            if (!IsUserAuthorized(userId))
            {
                _logger.LogWarning($"Unauthorized attempt to delete account {userId}.");
                return new ResponseDto<bool> { IsSuccess = false, Message = "Unauthorized to delete this account." };
            }

           try
           {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"DeleteAccountAsync: User not found for ID {userId}.");
                    return new ResponseDto<bool> { IsSuccess = false, Message = "User not found." };
                }

                if (user.IsDeactivated)
                {
                    _logger.LogInformation($"Attempt to delete already deactivated account {userId}.");
                    return new ResponseDto<bool> { IsSuccess = false, Message = "This account is already deactivated." };
                }

                // Schedule deletion after 30 days
                user.ScheduledDeletionDate = DateTime.UtcNow.AddDays(30);
                user.IsDeactivated = true;
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

                SignOutUser(userId);

                _logger.LogInformation($"User {userId} account scheduled for deletion in 30 days.");
                // Update the success message to reflect the 30-day retention period
                return new ResponseDto<bool> { IsSuccess = true, Message = "Your account has been scheduled for deletion and will be permanently removed after 30 days." };
           }
           catch (Exception ex)
           {
                _logger.LogError(ex, $"An error occurred while updating user {userId}.");
                return new ResponseDto<bool> { IsSuccess = false, Message = $"An error occurred: {ex.Message}" };
           }
        }

        public async Task<ResponseDto<bool>> ActivateAccountAsync(string userId)
        {
            var currentUserId = _httpContextAccessor.HttpContext.Items["UserId"];

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ResponseDto<bool> { IsSuccess = false, Message = "User not found." };
                }

                var loggedInUser = await _userManager.FindByIdAsync((string)currentUserId);
                if (!await _userManager.IsInRoleAsync(loggedInUser, "Admin"))
                {
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Unauthorized access." };
                }

                user.IsDeactivated = false;
                user.ScheduledDeletionDate = null;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Failed to activate account." };
                }

                return new ResponseDto<bool> { IsSuccess = true, Message = "Account activated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user {userId}.");
                return new ResponseDto<bool> { IsSuccess = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<ResponseDto<bool>> HardDeleteAccountAsync(string userId)
        {
            var currentUserId = _httpContextAccessor.HttpContext.Items["UserId"];

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new ResponseDto<bool> { IsSuccess = false, Message = "User not found." };
                }

                var loggedInUser = await _userManager.FindByIdAsync((string)currentUserId);
                if (!await _userManager.IsInRoleAsync(loggedInUser, "Admin"))
                {
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Unauthorized access." };
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Failed to delete account." };
                }

                return new ResponseDto<bool> { IsSuccess = true, Message = "Account deleted successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user {userId}.");
                return new ResponseDto<bool> { IsSuccess = false, Message = $"An error occurred: {ex.Message}" };
            }
        }



        private bool IsUserAuthorized(string userId)
        {
            var currentUser = _httpContextAccessor.HttpContext.User;
            var currentUserId = _httpContextAccessor.HttpContext.Items["UserId"];
            var isAdmin = currentUser.IsInRole("Admin");

            return isAdmin || userId.Equals(currentUserId);
        }

        private async void SignOutUser(string userId)
        {
            var currentUserId = _httpContextAccessor.HttpContext.Items["UserId"];

            if (userId.Equals(currentUserId))
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User has been signed out.");
            }
        }
    }
}
