using Exam.Application.Dto.Proctoring;
using Exam.Application.Services.Interfaces;
using Exam.Domain.Entities;
using Exam.Domain.Enum;
using Exam.Domain.Interface;
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
        private readonly IProctoringService _proctoringService;
        private readonly IUnitOfWork _unitOfWork;

        public ProctoringController(IProctoringService proctoringService, IUnitOfWork unitOfWork)
        {
            _proctoringService = proctoringService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Receives a camera frame and forwards it to the AI Proctoring Service.
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
        public async Task<IActionResult> ProcessFrame([FromForm] ProctoringFrameRequest request, CancellationToken cancellationToken)
        {
            // 1. Authentication & Authorization Validation
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int currentUserId))
            {
                // If authenticated, ensure the student is only reporting for themselves
                if (currentUserId != request.StudentId)
                {
                    return Forbid();
                }
            }

            // 2. Exam Session Validation
            // Check if there is an active (InProgress) session for this student and exam
            var sessions = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(es => es.ExamId == request.ExamId &&
                                 es.StudentId == request.StudentId &&
                                 es.Status == ExamStatus.InProgress);

            var activeSession = sessions.FirstOrDefault();

            if (activeSession == null)
            {
                return BadRequest(new { message = "No active exam session found. Proctoring is only allowed during an ongoing exam." });
            }

            // 3. Time Validation - Ensure the student is within their allotted exam time
            if (activeSession.EndDate.HasValue && DateTime.UtcNow > activeSession.EndDate.Value)
            {
                return BadRequest(new { message = "Exam session time has expired. Proctoring is no longer permitted." });
            }

            // 4. Forward to FastAPI Gateway
            try
            {
                var result = await _proctoringService.DetectCheatingAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (TimeoutException ex)
            {
                return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = ex.Message });
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "AI Service is temporarily unavailable.", detail = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during proctoring analysis.", error = ex.Message });
            }
        }

        /// <summary>
        /// Uploads a video file for continuous AI proctoring analysis.
        /// The video is processed frame-by-frame by the AI service with accumulated timers and scores.
        /// Confirmed violations are saved to the database for reporting.
        /// </summary>
        [HttpPost("video")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        [RequestSizeLimit(200_000_000)] // 200MB max
        public async Task<IActionResult> ProcessVideo([FromForm] ProctoringVideoRequest request, CancellationToken cancellationToken)
        {
            // 1. Authentication & Authorization Validation
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int currentUserId))
            {
                if (currentUserId != request.StudentId)
                {
                    return Forbid();
                }
            }

            // 2. Exam Session Validation
            var sessions = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(es => es.ExamId == request.ExamId &&
                                 es.StudentId == request.StudentId &&
                                 es.Status == ExamStatus.InProgress);

            var activeSession = sessions.FirstOrDefault();
            if (activeSession == null)
            {
                return BadRequest(new { message = "No active exam session found. Proctoring is only allowed during an ongoing exam." });
            }

            // 3. Forward video to FastAPI for processing
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
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "AI Service is temporarily unavailable.", detail = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during video proctoring analysis.", error = ex.Message });
            }
        }

    }
}
