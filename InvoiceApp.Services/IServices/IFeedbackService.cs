using InvoiceApp.Data.DTO;

namespace InvoiceApp.Services.IServices
{
    public interface IFeedbackService
    {
        Task<ResponseDto<string>> SubmitFeedbackAsync(FeedbackSubmitDto feedbackDto);
        Task<ResponseDto<IEnumerable<FeedbackResponseDto>>> GetAllFeedbackAsync(string userId);
        Task<ResponseDto<bool>> UpdateFeedbackStatusAsync(string feedbackId, FeedbackUpdateStatusDto updateDto);
        Task<ResponseDto<bool>> RespondToFeedbackAsync(string feedbackId, FeedbackResponseSubmissionDto responseDto);
    }
}
