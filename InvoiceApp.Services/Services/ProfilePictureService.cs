
using InvoiceApp.Data.DAO;
using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Models;
using InvoiceApp.Data.Models.IRepository;
using InvoiceApp.SD;
using InvoiceApp.Services.Exceptions;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvoiceApp.Services.Services
{
    public class ProfilePictureService : IProfilePictureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProfilePictureService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBlobRepository _blobRepository;
        private readonly BlobStorageSettings _blobStorageSettings;
        private readonly IConfiguration _configuration;

        public ProfilePictureService(IUnitOfWork unitOfWork, ILogger<ProfilePictureService> logger, IHttpContextAccessor httpContextAccessor, IBlobRepository blobRepository, IOptions<BlobStorageSettings> blobStorageSettings, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _blobRepository = blobRepository;
            _blobStorageSettings = blobStorageSettings.Value;
            _configuration = configuration;
        }

        public async Task<ResponseDto<string>> UploadProfilePicture(ProfilePictureUploadRequestDto request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.Items["UserId"] as string;
            if (string.IsNullOrEmpty(userId))
            {
                return new ResponseDto<string> { Message = "Invalid Request", IsSuccess = false };
            }

            ApplicationUser? user = await _unitOfWork.ApplicationUserRepository.GetByIdIncludingAsync(userId, user => user.ProfilePicture);
            _logger.LogInformation("Fetching details of this user with userId from the database - {0}", userId);

            if (user == null) throw new NotFoundException(nameof(ApplicationUser), userId);

            if (request.File == null || request.File?.Length == 0)
            {
                return await UpdateUserDetailsOnly(request, user, cancellationToken);
            }

            var extension = ValidateUploadedFile(request);
            var blobName = $"{userId}{extension}";
            BlobResponseDto blobResponse = await _blobRepository.UploadAsync(blobName,
                _blobStorageSettings.ProfilePictureContainer,
                request.File!);

            if (blobResponse.Error)
                return new ResponseDto<string>()
                {
                    IsSuccess = blobResponse.Error,
                    Message = blobResponse.Status,
                    Result = ErrorMessages.DefaultError
                };

            if (string.IsNullOrEmpty(blobResponse.Blob.Uri) || string.IsNullOrEmpty(blobResponse.Blob.Name))
                return new ResponseDto<string>()
                {
                    IsSuccess = false,
                    Message = "Invalid response from the Blob API",
                    Result = ErrorMessages.DefaultError
                };

            //update or create a record in the profile picture table as the case may be
            blobResponse.Blob.ContentType = extension;
            ProfilePicture? profilePicture =
                await _unitOfWork.ProfilePictureRepository.GetByUserIdAsync(userId);
            if (profilePicture == null)
            {
                //create a new record for the profile picture
                await AddProfilePicture(blobResponse, userId);
                //update the user
                UpdateUser(request, user);
                //save changes
                await _unitOfWork.SaveAsync(cancellationToken);
            }
            else
            {
                //update the user
                UpdateUserAndProfilePicture(request, user, blobResponse);
                //save changes
                await _unitOfWork.SaveAsync(cancellationToken);
            }

            return new ResponseDto<string>()
            {
                IsSuccess = true,
                Message = "Profile Picture Updated Successfully",
                Result = SuccessMessages.DefaultSuccess
            };
        }

        private async Task AddProfilePicture(BlobResponseDto blobResponse, string userId)
        {
            var profilePicture = ProfilePicture.Create(
                GetCdnLink(blobResponse.Blob),
                blobResponse.Blob.Name!,
                blobResponse.Blob.ContentType!,
                userId
            );
            profilePicture.Created_at = DateTime.Now;
            profilePicture.Created_by = userId;
            await _unitOfWork.ProfilePictureRepository.AddAsync(profilePicture);
        }

        private void UpdateUser(ProfilePictureUploadRequestDto request, ApplicationUser user)
        {
            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName))
                _unitOfWork.ApplicationUserRepository.UpdateAsync(user);
        }

        private void UpdateUserAndProfilePicture(ProfilePictureUploadRequestDto request, ApplicationUser user, BlobResponseDto blobResponse)
        {
            user.ProfilePicture!.Name = blobResponse.Blob.Name!;
            user.ProfilePicture.ImageData = GetCdnLink(blobResponse.Blob);
            user.ProfilePicture.ContentType = blobResponse.Blob.ContentType!;
            user.ProfilePicture.Updated_at = DateTime.Now;
            user.ProfilePicture.Updated_by = user.Id;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            _unitOfWork.ApplicationUserRepository.UpdateAsync(user);
        }

        private async Task<ResponseDto<string>> UpdateUserDetailsOnly(ProfilePictureUploadRequestDto request, ApplicationUser user, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.FirstName) && string.IsNullOrEmpty(request.LastName))
            {
                return new ResponseDto<string>()
                {
                    Message = "Invalid Request",
                    IsSuccess = false,
                    Result = ErrorMessages.DefaultError
                };
            }

            //attempt to update only the users
            UpdateUser(request, user);
            await _unitOfWork.SaveAsync(cancellationToken);
            return new ResponseDto<string>()
            {
                IsSuccess = true,
                Message = "User Details Update Successfully",
                Result = SuccessMessages.DefaultSuccess
            };
        }

        private static string ValidateUploadedFile(ProfilePictureUploadRequestDto request)
        {
            //validate the uploaded image
            const long maxFileSize = 5 * 1024 * 1024;
            string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };

            if (request.File?.Length > maxFileSize)
                throw new BadRequestException("File size exceeds the permissible limit of 5Mb");

            var extension = Path.GetExtension(request.File?.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
            {
                throw new BadRequestException("Invalid file type.");
            }

            return extension;
        }

        private string GetCdnLink(BlobDto blobDto)
        {
            // Extract the file name from the blob URI
            var fileName = new Uri(blobDto.Uri).AbsolutePath.Split('/').Last();

            // Construct the CDN URL
            var cdnUrl = $"{_configuration["CdnUrl"]}/profilepictures/{fileName}";
            return cdnUrl;
        }
    }
}
