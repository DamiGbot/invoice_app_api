using InvoiceApp.Data.DAO;
using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Enums;
using InvoiceApp.Data.Models;
using InvoiceApp.Data.Requests;
using InvoiceApp.Data.Responses;
using InvoiceApp.SD;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvoiceApp.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthService> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly JwtTokenSettings _jwtTokenSettings;

        public AuthService(UserManager<ApplicationUser> userManager, ILogger<AuthService> logger, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, JwtTokenSettings jwtTokenSettings)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _jwtTokenSettings = jwtTokenSettings;
        }

        public async Task<ResponseDto<AuthResponse>> Login(AuthRequest authRequest)
        {
            _logger.LogInformation("Processing login request {@AuthRequest}", authRequest);
            ResponseDto<AuthResponse> response = new() { IsSuccess = false, Message = "Invalid Login Attempt." };


            ApplicationUser? user = await _userManager.FindByEmailAsync(authRequest.Email);
            if (user == null)
            {
                _logger.LogWarning("Login - User {0} not found", authRequest.Email);
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, authRequest.Password, false, true);

            if (result.Succeeded)
            {
                IList<string> roles = await _userManager.GetRolesAsync(user);


                var jwtSecurityToken = _tokenService.CreateToken(user); 
                var newRefreshToken = _tokenService.GenerateRefreshToken(); 
                var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken); 

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_jwtTokenSettings.RefreshTokenValidityInDays);
                await _userManager.UpdateAsync(user);

                UserDto usertoReturn = new() 
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                };

                usertoReturn.Status = user.LockoutEnd >= DateTimeOffset.UtcNow
                ? UserStatus.Deactivated
                : user.EmailConfirmed
                    ? UserStatus.Active
                    : UserStatus.Inactive;

                var responseDto = new AuthResponse
                {
                    User = usertoReturn,
                    AccessToken = token,
                    Role = roles,
                    RefreshToken = newRefreshToken
                };

                response.IsSuccess = true;
                response.Message = "Login Successful";
                response.Result = responseDto;
                _logger.LogInformation("Login - User signed in successfully {0}", authRequest.Email);
                return response;
            }

            //check if the email is confirmed
            if (result.IsNotAllowed)
            {
                response.IsSuccess = false;
                response.Message = "Please check your email and confirm your email address, then try and login again.";
                _logger.LogInformation("Login - Email has not been confirmed {0}", authRequest.Email);
                return response;
            }

            //check if the account is locked out
            if (result.IsLockedOut)
            {
                response.IsSuccess = false;
                response.Message = "Your account is currently locked out. Please contact admin or reset your password.";
                _logger.LogInformation("Login - Account has been locked out {0}", authRequest.Email);
                return response;
            }

            return response;
        }

        public async Task<ResponseDto<string>> Register(RegistrationRequest registrationRequest)
        {
            _logger.LogInformation("Processing register request {@RegistrationRequestDto}", registrationRequest);
            ResponseDto<string> response = new();

            try
            {
                // register the user 
                ApplicationUser user = new()
                {
                    UserName = registrationRequest.Username,
                    Email = registrationRequest.Email,
                    FirstName = registrationRequest.FirstName,
                    LastName = registrationRequest.LastName,
                    CreatedOn = DateTime.Now
                };

                var registrationResult = await _userManager.CreateAsync(user, registrationRequest.Password!);

                if (registrationResult.Succeeded)
                {
                    var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    //// Create confirmation link 
                    //var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = emailConfirmationToken }, Request.Scheme);

                    // Send an email with the URL and token
                    // var resetUrl = $"https://{baseurl}/confirm-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
                    // send the email. 
                    // await _emailService.SendPasswordResetEmailAsync(user.Email, resetUrl);


                    IdentityResult roleResult = await AssignRole(user, registrationRequest.Role.ToString());

                    if (roleResult.Succeeded)
                    {
                        response.Message = "User Registration was Successful.";
                        response.IsSuccess = true;
                        response.Result = user.Id;
                        _logger.LogInformation("Register - User with this email registered successfully {0}",
                            registrationRequest.Email);
                    }
                    else
                    {
                        // Manual Rollback (Data Intergrity) 
                        var deletionResult = await _userManager.DeleteAsync(user);
                        if (deletionResult.Succeeded)
                        {
                            _logger.LogInformation("Rollback successful - User {Email} deleted after failing to assign role", user.Email);
                        }
                        else
                        {
                            _logger.LogError("Rollback failed - User {Email} creation could not be rolled back after failing to assign role", user.Email);
                        }

                        response.Message = roleResult.Errors.FirstOrDefault()?.Description ?? "An error occurred during role assignment.";
                        response.IsSuccess = false;
                        response.Result = ErrorMessages.DefaultError;

                        _logger.LogWarning("Register - An error occurred while registering this user {0} - {1}",
                            registrationRequest.Email, roleResult.Errors.FirstOrDefault()?.Description ?? "Undefined Error");
                    }

                    registrationRequest.Password = "";
                }
                else
                {
                    response.Message = registrationResult.Errors.FirstOrDefault()?.Description;
                    response.IsSuccess = false;
                    response.Result = ErrorMessages.DefaultError;

                    _logger.LogWarning("Register - An error occurred while registering this user {0} - {1}",
                        registrationRequest.Email, registrationResult.Errors.FirstOrDefault()?.Description ?? "Undefined Error");
                }
            } catch (Exception ex)
            {
                _logger.LogError("An error occurred during the registration process: {Message}", ex.Message);
                response.Message = $"An error occurred: {ex.Message}";
                response.IsSuccess = false;
            }

            return response;
        }

        public async Task<ResponseDto<string>> InitiatePasswordReset(InitiatePasswordResetDto request)
        {
            _logger.LogInformation("Processing password request {@InitiatePasswordResetDto}", request);
            ResponseDto<string> response = new()
            {
                IsSuccess = true,
                Message= "A notification will be sent to this email if an account is registered under it."
            };

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for unregistered email: {Email}", request.Email);
                return response; 
            }

            // check if the user is not locked out by the admin 
            if (user.IsLockedOutByAdmin)
            {
                response.IsSuccess = false;
                response.Message = "Your account has been deactivated. Please contact admin..";
                return response;
            }

            // send an email with the url and token!!!! 
            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Send an email with the URL and token
            // var resetUrl = $"https://{baseurl}/confirm-reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            // send the email. 
            // await _emailService.SendPasswordResetEmailAsync(user.Email, resetUrl);

            _logger.LogInformation("Password reset token generated for {Email}", user.Email); 
            return response;
        }

        public async Task<ResponseDto<string>> ConfirmPasswordReset(ConfirmPasswordResetDto request)
        {
            // log the message 
            _logger.LogInformation("Processing Confirm password request {@ConfirmPasswordResetDto}", request);
            ResponseDto<string> response = new()
            {
                IsSuccess = false,
                Message = "Invalid Password Reset Request"
            };

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                _logger.LogWarning("Invalid Password Reset requested for unregistered username: {UserName}", request.UserName);
                return response;
            }
            // reset password 
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset completed for this user {0}", request.UserName);
                response.IsSuccess = true;
                response.Message = "Your password has been reset. Please sign in.";
                return response;
            }

            response.Message = result.Errors.FirstOrDefault()?.Description;
            _logger.LogWarning("Password reset failed with an error {0} - {1}", request.UserName, response.Message ?? "Undefined identity error");

            return response;
        }

        public async Task<ResponseDto<UserDto>> ConfirmEmail(ConfirmEmailRequestDto request)
        {
            _logger.LogInformation("Confirm email request {@ConfirmEmailRequestDto}", request);
            ResponseDto<UserDto> response = new()
            {
                IsSuccess = false,
                Message = "Invalid Email Confirmation Request"
            };

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null) return response;

            // check if email is Confirmed 
            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (isEmailConfirmed)
            {
                response.Message =
                    "The email for this account has been confirmed already.";
                response.IsSuccess = false;

                return response;
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (result.Succeeded)
            {
                _logger.LogInformation("Email has been confirmed for this user {0}", request.UserName); 
                response.IsSuccess = true;
                response.Message = "Your email has been confirmed.";

                UserDto usertoReturn = new()
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                };

                response.Result = usertoReturn;
                return response; 
            }

            response.Message = result.Errors.FirstOrDefault()?.Description + " .Please contact the admin.";
            _logger.LogInformation("Email confirmation failed for this user {0} - {1}", request.UserName, response.Message ?? "Undefined Identity Error");
            return response;
        }

        public async Task<ResponseDto<RefreshTokenDto>> GetRefreshToken(RefreshTokenDto request)
        {
            ResponseDto<RefreshTokenDto> response =
                new() { IsSuccess = false, Message = "Invalid access token or refresh token" };
            ClaimsPrincipal principal = GetPrincipalFromExpiredToken(request.AccessToken);
            var email = principal.Claims.First(c => c.Type == ClaimTypes.Email).Value;

            //check that the username exists on the token
            if (string.IsNullOrEmpty(email)) return response;

            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                response.IsSuccess = false;
                response.Message = "Invalid access token or refresh token";
                return response;
            }

            //generate jwt token
            JwtSecurityToken jwtSecurityToken = _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_jwtTokenSettings.RefreshTokenValidityInDays);
            await _userManager.UpdateAsync(user);

            response.IsSuccess = true;
            response.Message = "Success";
            response.Result = new RefreshTokenDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                RefreshToken = newRefreshToken
            };

            return response;
        }

        private async Task<IdentityResult> AssignRole(ApplicationUser user, string roleName)
        {
            // Check if the role exists
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Error creating role {roleName}");
                }
            }
            // Attempt to add the user to the role
            return await _userManager.AddToRoleAsync(user, roleName);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenSettings.SymmetricSecurityKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal? principal =
                tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
