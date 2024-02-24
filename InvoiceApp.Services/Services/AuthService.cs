using InvoiceApp.Data.DAO;
using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Enums;
using InvoiceApp.Data.Models;
using InvoiceApp.Data.Requests;
using InvoiceApp.Data.Responses;
using InvoiceApp.SD;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
            // register the user 
            ApplicationUser user = new()
            {
                UserName = registrationRequest.Username,
                Email = registrationRequest.Email,
                FirstName = registrationRequest.FirstName,
                LastName = registrationRequest.LastName,
                CreatedOn = DateTime.Now
            };
            ResponseDto<string> response = new();

            var registrationResult = await _userManager.CreateAsync(user, registrationRequest.Password!);
            
            if (registrationResult.Succeeded)
            {
                IdentityResult roleResult = await AssignRole(user, registrationRequest.Role.ToString());

                if (roleResult.Succeeded)
                {
                    response.Message = "User Registration was Successful.";
                    response.IsSuccess = true;
                    response.Result = user.Id;
                    _logger.LogInformation("Register - User with this email registered successfully {0}",
                        registrationRequest.Email);
                } else
                {
                    response.Message = roleResult.Errors.FirstOrDefault()?.Description;
                    response.IsSuccess = false;
                    response.Result = ErrorMessages.DefaultError;

                    _logger.LogWarning("Register - An error occurred while registering this user {0} - {1}",
                        registrationRequest.Email, roleResult.Errors.FirstOrDefault()?.Description ?? "Undefined Error");
                }

                registrationRequest.Password = "";
            } else
            {
                response.Message = registrationResult.Errors.FirstOrDefault()?.Description;
                response.IsSuccess = false;
                response.Result = ErrorMessages.DefaultError;

                _logger.LogWarning("Register - An error occurred while registering this user {0} - {1}",
                    registrationRequest.Email, registrationResult.Errors.FirstOrDefault()?.Description ?? "Undefined Error");
            }

            return response;
        }

        public async Task<IdentityResult> AssignRole(ApplicationUser user, string roleName)
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

    }
}
