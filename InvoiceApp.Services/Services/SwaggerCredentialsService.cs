
using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Models;
using InvoiceApp.Data.Models.IRepository;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace InvoiceApp.Services.Services
{
    public class SwaggerCredentialsService : ISwaggerCredentialsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SwaggerCredentialsService> _logger;
        private readonly IPokemonService _pokemonService;

        public SwaggerCredentialsService(IUnitOfWork unitOfWork, ILogger<SwaggerCredentialsService> logger, IPokemonService pokemonService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _pokemonService = pokemonService;
        }

        public async Task<ResponseDto<SwaggerCredentialResponseDto>> GenerateCredentialsAsync(SwaggerCredentialRequestDto requestDto)
        {

            ResponseDto<SwaggerCredentialResponseDto> response = new();

            var username = requestDto == null ? await _pokemonService.GetRandomPokemonNameAsync() : requestDto.UserName;
            
            try
            {
                _logger.LogInformation($"Attempting to generate new credentials for {username}");

                // Generate a random password
                var password = GenerateRandomPassword();
                var expiryTime = DateTime.Now.AddHours(24);
                var passwordHasher = new PasswordHasher<object?>();

                var swaggerCredential = new SwaggerCredential
                {
                    Username = username,
                    Password = passwordHasher.HashPassword(null, password),
                    ExpiryTime = expiryTime
                };

                // Store the credentials with expiry time
                await _unitOfWork.SwaggerCredentialRepository.AddAsync(swaggerCredential);
                await _unitOfWork.SaveAsync(CancellationToken.None);

                _logger.LogInformation($"Credentials successfully generated for {username}");

                var result = new SwaggerCredentialResponseDto
                {
                    UserName = username,
                    Password = password,
                    ExpiryTime = expiryTime
                };

                response.Result = result;
                response.IsSuccess = true;
                response.Message = "Swagger Credential successfully created and will last for 24 hours. Please note the password is shown only once and cannot be retrieved again.";

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while generating credentials for {username}");
                response.IsSuccess = false;
                response.Message = "An unexpected error occurred while creating credentials. Please try again later or contact support if the problem persists.";
            }

            return response;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                _logger.LogInformation($"Attempting to validate credentials for {username}");

                var credentials = await _unitOfWork.SwaggerCredentialRepository.GetByUsernameAsync(username);

                if (credentials == null)
                {
                    _logger.LogWarning($"Credentials not found for {username}");
                    return false;
                }

                if (credentials.ExpiryTime < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Credentials expired for {username}");
                    return false; 
                }

                var passwordHasher = new PasswordHasher<object?>();
                var verificationResult = passwordHasher.VerifyHashedPassword(null, credentials.Password, password);

                if (verificationResult == PasswordVerificationResult.Success)
                {
                    _logger.LogInformation($"Credentials validated for {username}");
                    return true; 
                }
                else
                {
                    _logger.LogWarning($"Invalid credentials provided for {username}");
                    return false; 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while validating credentials for {username}");
                return false;
            }
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numberChars = "0123456789";
            const string specialChars = "!@#$%^&*";
            string allChars = lowerChars + upperChars + numberChars + specialChars;

            Random random = new Random();
            
            using (RNGCryptoServiceProvider rng = new())
            {
                byte[] data = new byte[length];
                rng.GetBytes(data);
                char[] password = new char[length];

                for (int i = 0; i < length; i++)
                {
                    password[i] = allChars[data[i] % allChars.Length];
                }

                password[random.Next(0, length)] = lowerChars[random.Next(0, lowerChars.Length)];
                password[random.Next(0, length)] = upperChars[random.Next(0, upperChars.Length)];
                password[random.Next(0, length)] = numberChars[random.Next(0, numberChars.Length)];
                password[random.Next(0, length)] = specialChars[random.Next(0, specialChars.Length)];

                return new string(password);
            }
        }
    }
}
