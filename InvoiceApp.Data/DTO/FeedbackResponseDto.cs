namespace InvoiceApp.Data.DTO
{
    public class FeedbackResponseDto : FeedbackSubmitDto
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string AdminComments { get; set; }
    }
}
