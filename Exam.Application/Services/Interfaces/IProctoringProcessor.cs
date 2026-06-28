using Exam.Application.Dto.Proctoring;
using Microsoft.AspNetCore.Http;

namespace Exam.Application.Services.Interfaces
{
    /// <summary>
    /// Shared proctoring processing pipeline used by both the HTTP controller
    /// (POST /api/Proctoring/frame) and the WebSocket handler (/ws/proctoring/frame).
    ///
    /// Responsibilities:
    ///   1. Validate student identity (prevent impersonation).
    ///   2. Validate that an active exam session exists for the student.
    ///   3. Validate the session time boundary.
    ///   4. Forward the frame to the AI service via IProctoringService.
    ///
    /// The underlying ProctoringService, database save logic, and reporting
    /// pipeline are NOT modified by this interface.
    /// </summary>
    public interface IProctoringProcessor
    {
        /// <summary>
        /// Validates the request and runs the full proctoring pipeline for a single frame.
        /// </summary>
        /// <param name="studentId">ID of the student being proctored.</param>
        /// <param name="examId">ID of the active exam.</param>
        /// <param name="frame">The camera frame as an IFormFile (JPEG/PNG).</param>
        /// <param name="currentUserId">Authenticated user ID extracted from the JWT claim.</param>
        /// <param name="cancellationToken">Propagated from the transport layer.</param>
        /// <returns>
        /// A result tuple:
        ///   Success     – true when the frame was processed without errors.
        ///   Result      – the FastAPI AI response (null on failure).
        ///   ErrorMessage – human-readable error string (null on success).
        ///   StatusCode  – HTTP-equivalent status code for mapping to WS error codes.
        /// </returns>
        Task<ProctoringProcessorResult> ProcessFrameAsync(
            int studentId,
            int examId,
            IFormFile frame,
            int currentUserId,
            CancellationToken cancellationToken);
    }

    /// <summary>Result returned from IProctoringProcessor.ProcessFrameAsync.</summary>
    public sealed class ProctoringProcessorResult
    {
        public bool Success { get; init; }
        public FastApiResponseDto? Result { get; init; }
        public string? ErrorMessage { get; init; }
        public string? ErrorCode { get; init; }

        /// <summary>HTTP-equivalent status code (200, 400, 403, 502, 504, 500).</summary>
        public int StatusCode { get; init; }

        // ── Factory helpers ────────────────────────────────────────────────────

        public static ProctoringProcessorResult Ok(FastApiResponseDto result) =>
            new() { Success = true, Result = result, StatusCode = 200 };

        public static ProctoringProcessorResult Forbidden(string message) =>
            new() { Success = false, ErrorMessage = message, ErrorCode = "FORBIDDEN", StatusCode = 403 };

        public static ProctoringProcessorResult BadRequest(string message, string code = "BAD_REQUEST") =>
            new() { Success = false, ErrorMessage = message, ErrorCode = code, StatusCode = 400 };

        public static ProctoringProcessorResult GatewayTimeout(string message) =>
            new() { Success = false, ErrorMessage = message, ErrorCode = "AI_TIMEOUT", StatusCode = 504 };

        public static ProctoringProcessorResult BadGateway(string message) =>
            new() { Success = false, ErrorMessage = message, ErrorCode = "AI_UNAVAILABLE", StatusCode = 502 };

        public static ProctoringProcessorResult InternalError(string message) =>
            new() { Success = false, ErrorMessage = message, ErrorCode = "INTERNAL_ERROR", StatusCode = 500 };
    }
}
