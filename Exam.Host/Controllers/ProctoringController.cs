using Exam.Application.Dto.Proctoring;
using Exam.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exam.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProctoringController : ControllerBase
    {
        private readonly IProctoringProcessor _processor;
        private readonly IProctoringService   _proctoringService; // kept for ProcessVideo

        public ProctoringController(
            IProctoringProcessor processor,
            IProctoringService   proctoringService)
        {
            _processor         = processor;
            _proctoringService = proctoringService;
        }

        /// <summary>
        /// Receives a camera frame and forwards it to the AI Proctoring Service.
        /// All validation and forwarding is now handled by IProctoringProcessor,
        /// which is the same pipeline used by the WebSocket endpoint.
        /// </summary>
        /// <param name="request">Multipart form data containing the frame image, student ID, and exam ID.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>AI detection results and risk assessment.</returns>
        [HttpPost("frame")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(FastApiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
        public async Task<IActionResult> ProcessFrame(
            [FromForm] ProctoringFrameRequest request,
            CancellationToken cancellationToken)
        {
            // Resolve the authenticated user's ID from JWT claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int currentUserId))
                return Unauthorized(new { message = "Cannot determine authenticated user identity." });

            // Delegate all validation + AI call to the shared processor
            var result = await _processor.ProcessFrameAsync(
                request.StudentId,
                request.ExamId,
                request.Frame,
                currentUserId,
                cancellationToken);

            if (result.Success)
                return Ok(result.Result);

            return result.StatusCode switch
            {
                403 => Forbid(),
                400 => BadRequest(new { message = result.ErrorMessage, code = result.ErrorCode }),
                504 => StatusCode(StatusCodes.Status504GatewayTimeout,
                           new { message = result.ErrorMessage }),
                502 => StatusCode(StatusCodes.Status502BadGateway,
                           new { message = result.ErrorMessage }),
                _   => StatusCode(StatusCodes.Status500InternalServerError,
                           new { message = result.ErrorMessage })
            };
        }

        /// <summary>
        /// Uploads a video file for continuous AI proctoring analysis.
        /// The video is processed frame-by-frame by the AI service with accumulated
        /// timers and scores. Confirmed violations are saved to the database for reporting.
        /// </summary>
        [HttpPost("video")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [RequestSizeLimit(200_000_000)] // 200 MB max
        public async Task<IActionResult> ProcessVideo(
            [FromForm] ProctoringVideoRequest request,
            CancellationToken cancellationToken)
        {
            // 1. Authentication & Authorization Validation
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int currentUserId))
            {
                if (currentUserId != request.StudentId)
                    return Forbid();
            }

            // 2. Forward video to FastAPI for processing (unchanged)
            try
            {
                var result = await _proctoringService.ProcessVideoAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (TimeoutException ex)
            {
                return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = ex.Message });
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "AI Service is temporarily unavailable.", detail = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred during video proctoring analysis.", error = ex.Message });
            }
        }
    }
}
