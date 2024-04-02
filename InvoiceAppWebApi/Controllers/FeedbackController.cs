using InvoiceApp.Data.DTO;
using InvoiceApp.SD;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace InvoiceApp.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(CustomProblemDetails))]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Submits user feedback.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/feedback
        ///     {
        ///        "category": "Bug Report",
        ///        "description": "Detailed description of the feedback or issue encountered.",
        ///        "additionalInfo": "Any additional information or context that might help in understanding the feedback."
        ///     }
        ///
        /// </remarks>
        /// <param name="feedbackDto">Feedback submission DTO</param>
        /// <returns>A newly created feedback</returns>
        /// <response code="201">Returns the newly created feedback ID</response>
        /// <response code="400">If the item is null</response> 
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ResponseDto<string>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Submits feedback.")]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackSubmitDto feedbackDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _feedbackService.SubmitFeedbackAsync(feedbackDto);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return CreatedAtAction(nameof(SubmitFeedback), new { id = response.Result }, response);
        }

        /// <summary>
        /// Retrieves all feedback (Admin only).
        /// </summary>
        /// <returns>All feedback entries</returns>
        /// <response code="200">Returns all feedback entries</response>
        /// <response code="401">If the user is unauthorized</response>
        /// <response code="403">If the user is not an admin</response>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ResponseDto<IEnumerable<FeedbackResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = "Retrieves all feedback. Admin only.")]
        public async Task<IActionResult> GetAllFeedback([FromBody] FeedbackRequestDto feedbackRequestDto)
        {
            var response = await _feedbackService.GetAllFeedbackAsync(feedbackRequestDto.userId);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPut("{feedbackId}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = "Updates feedback status. Admin only.")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateFeedbackStatus(string feedbackId, [FromBody] FeedbackUpdateStatusDto updateDto)
        {
            var response = await _feedbackService.UpdateFeedbackStatusAsync(feedbackId, updateDto);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("{feedbackId}/response")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(Summary = "Responds to feedback. Admin only.")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RespondToFeedback(string feedbackId, [FromBody] FeedbackResponseSubmissionDto responseDto)
        {
            var response = await _feedbackService.RespondToFeedbackAsync(feedbackId, responseDto);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
