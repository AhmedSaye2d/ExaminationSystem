using Exam.Application.Dto.Proctoring;
using Exam.Application.Services.Interfaces;
using Exam.Domain.Entities;
using Exam.Domain.Enum;
using Exam.Domain.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Exam.Infrastructure.Services
{
    /// <summary>
    /// Implements IProctoringProcessor — the shared validation and forwarding
    /// pipeline used by both:
    ///   • POST /api/Proctoring/frame  (ProctoringController)
    ///   • WS   /ws/proctoring/frame  (ProctoringWebSocketHandler)
    ///
    /// This class contains ONLY the cross-cutting validation logic that was
    /// previously duplicated inside ProctoringController.ProcessFrame.
    /// All AI calling, database saving, throttling, and reporting logic
    /// remains exclusively inside ProctoringService — unchanged.
    /// </summary>
    public class ProctoringProcessor : IProctoringProcessor
    {
        private readonly IProctoringService _proctoringService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProctoringProcessor> _logger;

        public ProctoringProcessor(
            IProctoringService proctoringService,
            IUnitOfWork unitOfWork,
            ILogger<ProctoringProcessor> logger)
        {
            _proctoringService = proctoringService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ProctoringProcessorResult> ProcessFrameAsync(
            int studentId,
            int examId,
            IFormFile frame,
            int currentUserId,
            CancellationToken cancellationToken)
        {
            // ── 1. Identity Guard ──────────────────────────────────────────────
            // A student may only submit frames for themselves.
            if (currentUserId != studentId)
            {
                _logger.LogWarning(
                    "Identity mismatch: authenticated user {CurrentUser} tried to submit frame for student {StudentId}",
                    currentUserId, studentId);

                return ProctoringProcessorResult.Forbidden(
                    "You are not authorized to submit frames for another student.");
            }

            // ── 2. Active Exam Session Validation ─────────────────────────────
            var sessions = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(es => es.ExamId == examId &&
                                 es.StudentId == studentId &&
                                 es.Status == ExamStatus.InProgress);

            var activeSession = sessions.FirstOrDefault();

            if (activeSession == null)
            {
                _logger.LogWarning(
                    "No active session for student {StudentId}, exam {ExamId}", studentId, examId);

                return ProctoringProcessorResult.BadRequest(
                    "No active exam session found. Proctoring is only allowed during an ongoing exam.",
                    "NO_ACTIVE_SESSION");
            }

            // ── 3. Time Boundary Validation ───────────────────────────────────
            if (activeSession.EndDate.HasValue && DateTime.UtcNow > activeSession.EndDate.Value)
            {
                _logger.LogWarning(
                    "Exam session expired for student {StudentId}, exam {ExamId}", studentId, examId);

                return ProctoringProcessorResult.BadRequest(
                    "Exam session time has expired. Proctoring is no longer permitted.",
                    "SESSION_EXPIRED");
            }

            // ── 4. Forward to AI Service ──────────────────────────────────────
            try
            {
                var request = new ProctoringFrameRequest
                {
                    Frame     = frame,
                    StudentId = studentId,
                    ExamId    = examId
                };

                var result = await _proctoringService.DetectCheatingAsync(request, cancellationToken);

                _logger.LogDebug(
                    "Frame processed — Student {StudentId}, Exam {ExamId}, Cheating: {Cheating}",
                    studentId, examId, result.Cheating);

                return ProctoringProcessorResult.Ok(result);
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex,
                    "AI timeout for student {StudentId}, exam {ExamId}", studentId, examId);

                return ProctoringProcessorResult.GatewayTimeout(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "AI service unreachable for student {StudentId}, exam {ExamId}", studentId, examId);

                return ProctoringProcessorResult.BadGateway(
                    "AI Service is temporarily unavailable. " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error processing frame for student {StudentId}, exam {ExamId}",
                    studentId, examId);

                return ProctoringProcessorResult.InternalError(
                    "An error occurred during proctoring analysis. " + ex.Message);
            }
        }
    }
}
