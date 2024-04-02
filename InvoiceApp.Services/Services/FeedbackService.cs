using AutoMapper;
using InvoiceApp.Data.DTO;
using InvoiceApp.Data.Models.IRepository;
using InvoiceApp.Data.Models;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace InvoiceApp.Services.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FeedbackService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FeedbackService(IUnitOfWork unitOfWork, ILogger<FeedbackService> logger, UserManager<ApplicationUser> userManager, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseDto<string>> SubmitFeedbackAsync(FeedbackSubmitDto feedbackDto)
        {
            var userId = (string)_httpContextAccessor.HttpContext.Items["UserId"];
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var feedback = _mapper.Map<Feedback>(feedbackDto);
                feedback.SubmittedBy = userId;
                feedback.SubmittedOn = DateTime.UtcNow;
                feedback.Created_at = DateTime.Now;
                feedback.Created_by = userId;

                await _unitOfWork.FeedbackRepository.AddAsync(feedback);
                await _unitOfWork.SaveAsync(CancellationToken.None);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Feedback submitted successfully. User: {userId}, Feedback ID: {feedback.Id}");
                return new ResponseDto<string> { IsSuccess = true, Message = "Feedback submitted successfully.", Result = feedback.Id };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error submitting feedback. User: {UserId}", userId);
                return new ResponseDto<string> { IsSuccess = false, Message = "An error occurred while submitting feedback." };
            }
        }

        public async Task<ResponseDto<IEnumerable<FeedbackResponseDto>>> GetAllFeedbackAsync(string? userId)
        {
            var currentUserId = _httpContextAccessor.HttpContext.Items["UserId"];
            try
            {
                var loggedInUser = await _userManager.FindByIdAsync((string)currentUserId);
                if (!await _userManager.IsInRoleAsync(loggedInUser, "Admin"))
                {
                    _logger.LogWarning("Unauthorized access attempt to fetch all feedback. User: {UserId}", currentUserId);
                    return new ResponseDto<IEnumerable<FeedbackResponseDto>> { IsSuccess = false, Message = "Unauthorized access." };
                }

                var feedbackItems = await _unitOfWork.FeedbackRepository.GetAllAsync();

                
                if (userId != null)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        _logger.LogWarning("User doesn't exist!!");
                        return new ResponseDto<IEnumerable<FeedbackResponseDto>> { IsSuccess = false, Message = "User not found." };
                    }
                    feedbackItems = feedbackItems.Where(item => item.SubmittedBy.Equals(userId));
                }

                var feedbackDtos = _mapper.Map<IEnumerable<FeedbackResponseDto>>(feedbackItems);

                return new ResponseDto<IEnumerable<FeedbackResponseDto>> { IsSuccess = true, Result = feedbackDtos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all feedback. User: {UserId}", userId);
                return new ResponseDto<IEnumerable<FeedbackResponseDto>> { IsSuccess = false, Message = "An error occurred while fetching feedback." };
            }
        }

        public async Task<ResponseDto<bool>> UpdateFeedbackStatusAsync(string feedbackId, FeedbackUpdateStatusDto updateDto)
        {
            var currentUserId = _httpContextAccessor.HttpContext.Items["UserId"];
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var loggedInUser = await _userManager.FindByIdAsync((string)currentUserId);
                if (!await _userManager.IsInRoleAsync(loggedInUser, "Admin"))
                {
                    _logger.LogWarning("Unauthorized attempt to update feedback status. User: {UserId}, Feedback ID: {feedbackId}", currentUserId, feedbackId);
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Unauthorized access." };
                }

                var feedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(feedbackId);
                if (feedback == null)
                {
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Feedback not found." };
                }

                feedback.Status = updateDto.Status;
                if (!string.IsNullOrWhiteSpace(updateDto.AdminComments)) feedback.AdminComments = updateDto.AdminComments;
                feedback.Updated_by = (string)currentUserId;
                feedback.Updated_at = DateTime.UtcNow;
                await _unitOfWork.FeedbackRepository.UpdateAsync(feedback);
                await _unitOfWork.SaveAsync(CancellationToken.None);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Feedback status updated successfully. Feedback ID: {feedbackId}, Status: {updateDto.Status}");
                return new ResponseDto<bool> { IsSuccess = true, Message = "Feedback status updated successfully.", Result = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feedback status. Feedback ID: {FeedbackId}", feedbackId);
                return new ResponseDto<bool> { IsSuccess = false, Message = "An error occurred while updating feedback status." };
            }
        }

        public async Task<ResponseDto<bool>> RespondToFeedbackAsync(string feedbackId, FeedbackResponseSubmissionDto responseDto)
        {
            var currentUserId = _httpContextAccessor.HttpContext.Items["UserId"];
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var loggedInUser = await _userManager.FindByIdAsync((string)currentUserId);
                if (!await _userManager.IsInRoleAsync(loggedInUser, "Admin"))
                {
                    _logger.LogWarning("Unauthorized attempt to respond to feedback. User: {UserId}, Feedback ID: {feedbackId}", currentUserId, feedbackId);
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Unauthorized access." };
                }

                var feedback = await _unitOfWork.FeedbackRepository.GetByIdAsync(feedbackId);
                if (feedback == null)
                {
                    return new ResponseDto<bool> { IsSuccess = false, Message = "Feedback not found." };
                }

                feedback.AdminComments = responseDto.Response;
                await _unitOfWork.FeedbackRepository.UpdateAsync(feedback);
                await _unitOfWork.SaveAsync(CancellationToken.None);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation($"Response to feedback submitted successfully. Feedback ID: {feedbackId}");
                return new ResponseDto<bool> { IsSuccess = true, Message = "Response to feedback submitted successfully.", Result = true};
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to feedback. Feedback ID: {FeedbackId}", feedbackId);
                return new ResponseDto<bool> { IsSuccess = false, Message = "An error occurred while responding to feedback." };
            }
        }
    }
}
