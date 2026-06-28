using Exam.Application.Dto.Proctoring;
using Exam.Application.Services.Interfaces;
using Exam.Domain.Entities;
using Exam.Domain.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Exam.Infrastructure.Services
{
    public class ProctoringService : IProctoringService
    {
        private readonly HttpClient _httpClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProctoringService> _logger;
        private readonly string _fastApiBaseUrl;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(30);

        public ProctoringService(
            HttpClient httpClient,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            ILogger<ProctoringService> logger,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _fastApiBaseUrl = configuration["FastApi:BaseUrl"] ?? "http://localhost:8000";
            _configuration = configuration;
            _cache = cache;
            // Load heartbeat interval from config if available
            var heartbeatConfig = _configuration["Proctoring:HeartbeatIntervalSeconds"];
            if (!string.IsNullOrEmpty(heartbeatConfig) && int.TryParse(heartbeatConfig, out int heartbeatSeconds))
            {
                _heartbeatInterval = TimeSpan.FromSeconds(heartbeatSeconds);
            }
        }

        public async Task<FastApiResponseDto> DetectCheatingAsync(ProctoringFrameRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var content = new MultipartFormDataContent();

                // 1. Prepare Frame Image
                var stream = request.Frame.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(request.Frame.ContentType);
                content.Add(streamContent, "file", request.Frame.FileName);

                // 2. Add Student and Exam IDs
                content.Add(new StringContent(request.StudentId.ToString()), "student_id");
                content.Add(new StringContent(request.ExamId.ToString()), "exam_id");

                // 3. Send to FastAPI
                _logger.LogInformation("Sending frame for Student {StudentId}, Exam {ExamId} to FastAPI", request.StudentId, request.ExamId);

                var response = await _httpClient.PostAsync($"{_fastApiBaseUrl}/api/v1/detect-cheating", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("FastAPI error: {StatusCode} - {Error}", response.StatusCode, errorMsg);
                    throw new HttpRequestException($"AI Service error: {response.StatusCode}");
                }

                // 4. Log Raw Response for Debugging
                var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("FastAPI Raw Response: {RawResponse}", rawResponse);

                // 5. Deserialize
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<FastApiResponseDto>(rawResponse, options);

                if (apiResponse == null)
                {
                    throw new Exception("Received empty response from AI Service");
                }

                // 4. Save to Database - Optimized: Only save when cheating detected or periodic heartbeat
                if (ShouldSaveLog(request.StudentId, request.ExamId, apiResponse))
                {
                    await SaveProctoringLogAsync(request, apiResponse);


                }

                return apiResponse;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("FastAPI request timed out for Student {StudentId}", request.StudentId);
                throw new TimeoutException("The AI Proctoring service timed out. Please check your connection.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ProctoringService for Student {StudentId}", request.StudentId);
                throw;
            }
        }

        /// <summary>
        /// Determines whether a proctoring log should be saved.
        /// FastAPI is the sole authority for cheating decisions — this method never modifies apiResponse.
        /// </summary>
        private bool ShouldSaveLog(int studentId, int examId, FastApiResponseDto apiResponse)
        {
            if (apiResponse == null) return false;

            var confirmedViolations = new[] { "phone_detected", "extra_person", "no_face", "head_violation", "gaze_violation" };

            // Clear cache for violations that are no longer active
            var activeEvents = apiResponse.Events ?? new List<string>();
            if (!string.IsNullOrEmpty(apiResponse.CurrentEvent) && !activeEvents.Contains(apiResponse.CurrentEvent))
            {
                activeEvents.Add(apiResponse.CurrentEvent);
            }

            foreach (var violation in confirmedViolations)
            {
                if (!apiResponse.Cheating || !activeEvents.Contains(violation))
                {
                    var cacheKeyToClear = $"event_{studentId}_{examId}_{violation}";
                    _cache.Remove(cacheKeyToClear);
                }
            }

            // Only persist logs when FastAPI explicitly flags a cheating event.
            if (!apiResponse.Cheating || string.IsNullOrEmpty(apiResponse.CurrentEvent))
                return false;

            if (!confirmedViolations.Contains(apiResponse.CurrentEvent))
                return false;

            // Throttle: deduplicate the same event within a 10-second window.
            var cacheKey = $"event_{studentId}_{examId}_{apiResponse.CurrentEvent}";
            if (_cache.TryGetValue(cacheKey, out _))
                return false;

            _cache.Set(cacheKey, true, TimeSpan.FromSeconds(10));
            return true;
        }

        /// <summary>
        /// Persists the proctoring log using exactly the values returned by FastAPI.
        /// ASP.NET Core never alters cheating decisions, scores, or risk levels.
        /// </summary>
        private async Task SaveProctoringLogAsync(ProctoringFrameRequest request, FastApiResponseDto apiResponse)
        {
            // --- Evidence image ---
            string? evidencePath = null;
            try
            {
                var eventName = apiResponse.CurrentEvent ?? "unknown";
                var timestampStr = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{eventName}_{timestampStr}.jpg";
                var relativeDir = Path.Combine("wwwroot", "evidence", $"exam_{request.ExamId}", $"student_{request.StudentId}");
                var absoluteDir = Path.Combine(Directory.GetCurrentDirectory(), relativeDir);

                if (!Directory.Exists(absoluteDir))
                    Directory.CreateDirectory(absoluteDir);

                var absoluteFilePath = Path.Combine(absoluteDir, fileName);

                using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create))
                using (var frameStream = request.Frame.OpenReadStream())
                {
                    if (frameStream.CanSeek) frameStream.Position = 0;
                    await frameStream.CopyToAsync(fileStream);
                }

                // Relative path served via static files middleware.
                evidencePath = $"/evidence/exam_{request.ExamId}/student_{request.StudentId}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save evidence image for Student {StudentId}", request.StudentId);
            }

            // --- Persist exactly what FastAPI returned — no overrides ---
            var log = new ProctoringLog
            {
                StudentId       = request.StudentId,
                ExamId          = request.ExamId,
                Cheating        = apiResponse.Cheating,
                CurrentEvent    = apiResponse.CurrentEvent,
                RiskLevel       = apiResponse.Session?.RiskLevel ?? "Unknown",
                TotalScore      = apiResponse.Session?.TotalScore ?? 0,
                SuspiciousTime  = apiResponse.Session?.SuspiciousTime ?? 0,
                EventsJson      = JsonSerializer.Serialize(apiResponse),
                Timestamp       = DateTime.UtcNow,
                EvidenceImagePath = evidencePath,

                // Granular sensor data (read-only mirror of FastAPI output)
                PhoneDetected   = apiResponse.PhoneDetection?.Detected ?? false,
                PhoneConfidence = apiResponse.PhoneDetection?.Confidence ?? 0,
                PersonCount     = apiResponse.PersonDetection?.PersonCount ?? 0,
                EyeStatus       = apiResponse.EyeTracking?.Status,
                HeadStatus      = apiResponse.HeadPose?.Status,
                FacePresent     = apiResponse.FaceDetection?.FacePresent ?? false
            };

            await _unitOfWork.Repository<ProctoringLog>().AddAsync(log);
            await _unitOfWork.CompleteAsync();

            _logger.LogDebug(
                "Proctoring log saved — Student {StudentId}, Exam {ExamId}, Event: {Event}, Cheating: {Cheating}, Risk: {Risk}",
                request.StudentId, request.ExamId, log.CurrentEvent, log.Cheating, log.RiskLevel);
        }

        public async Task<object> ProcessVideoAsync(ProctoringVideoRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var content = new MultipartFormDataContent();

                // 1. Prepare Video File
                var stream = request.Video.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(request.Video.ContentType);
                content.Add(streamContent, "file", request.Video.FileName);

                // 2. Add Student, Exam IDs, and Interval
                content.Add(new StringContent(request.StudentId.ToString()), "student_id");
                content.Add(new StringContent(request.ExamId.ToString()), "exam_id");
                content.Add(new StringContent(request.Interval.ToString(System.Globalization.CultureInfo.InvariantCulture)), "interval");

                // 3. Send to FastAPI
                _logger.LogInformation("Sending video for Student {StudentId}, Exam {ExamId} to FastAPI", request.StudentId, request.ExamId);

                // Assuming FastAPI exposes /api/v1/detect-cheating-video
                var response = await _httpClient.PostAsync($"{_fastApiBaseUrl}/api/v1/detect-cheating-video", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("FastAPI error: {StatusCode} - {Error}", response.StatusCode, errorMsg);
                    throw new HttpRequestException($"AI Service error: {response.StatusCode}");
                }

                // 4. Return the parsed response object
                var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<object>(rawResponse, options);

                return apiResponse ?? new object();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("FastAPI request timed out for Student {StudentId}", request.StudentId);
                throw new TimeoutException("The AI Proctoring service timed out. Please check your connection.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in ProctoringService.ProcessVideoAsync for Student {StudentId}", request.StudentId);
                throw;
            }
        }

    }
}
