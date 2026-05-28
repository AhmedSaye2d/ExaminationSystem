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

        private bool ShouldSaveLog(int studentId, int examId, FastApiResponseDto apiResponse)
        {
            if (apiResponse == null) return false;

            var confirmedViolations = new[] { "phone_detected", "extra_person", "no_face", "head_violation", "gaze_violation" };

            if (apiResponse.Cheating && apiResponse.CurrentEvent != null && confirmedViolations.Contains(apiResponse.CurrentEvent))
            {
                double timer = 0;
                switch (apiResponse.CurrentEvent)
                {
                    case "phone_detected": timer = apiResponse.PhoneDetection?.TimerSeconds ?? 0; break;
                    case "extra_person": timer = apiResponse.PersonDetection?.TimerSeconds ?? 0; break;
                    case "no_face": timer = apiResponse.FaceDetection?.TimerSeconds ?? 0; break;
                    case "gaze_violation": timer = apiResponse.EyeTracking?.TimerSeconds ?? 0; break;
                }

                bool isLowRisk = string.Equals(apiResponse.Session?.RiskLevel, "LOW", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(apiResponse.Session?.RiskLevel, "LOW RISK", StringComparison.OrdinalIgnoreCase);

                bool isValidViolation = timer >= 2.0 || 
                                        (apiResponse.Session?.TotalScore > 0) || 
                                        !isLowRisk;

                if (!isValidViolation)
                {
                    return false;
                }

                string eventCacheKey = $"event_{studentId}_{examId}_{apiResponse.CurrentEvent}";
                if (!_cache.TryGetValue(eventCacheKey, out _))
                {
                    _cache.Set(eventCacheKey, true, TimeSpan.FromSeconds(10));
                    return true;
                }
            }

            return false;
        }

        private async Task SaveProctoringLogAsync(ProctoringFrameRequest request, FastApiResponseDto apiResponse)
        {
            string? evidencePath = null;
            try
            {
                var eventName = apiResponse.CurrentEvent ?? "unknown";
                var timestampStr = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{eventName}_{timestampStr}.jpg";
                var relativeDir = Path.Combine("wwwroot", "evidence", $"exam_{request.ExamId}", $"student_{request.StudentId}");
                var absoluteDir = Path.Combine(Directory.GetCurrentDirectory(), relativeDir);

                if (!Directory.Exists(absoluteDir))
                {
                    Directory.CreateDirectory(absoluteDir);
                }

                var absoluteFilePath = Path.Combine(absoluteDir, fileName);

                using (var fileStream = new FileStream(absoluteFilePath, FileMode.Create))
                {
                    using (var frameStream = request.Frame.OpenReadStream())
                    {
                        if (frameStream.CanSeek) frameStream.Position = 0;
                        await frameStream.CopyToAsync(fileStream);
                    }
                }

                // Store a relative path that can be served via static files
                evidencePath = $"/evidence/exam_{request.ExamId}/student_{request.StudentId}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save evidence image for student {StudentId}", request.StudentId);
            }

            var log = new ProctoringLog
            {
                StudentId = request.StudentId,
                ExamId = request.ExamId,
                CurrentEvent = apiResponse.CurrentEvent,
                EventsJson = JsonSerializer.Serialize(apiResponse),
                RiskLevel = apiResponse.Session?.RiskLevel ?? "Unknown",
                TotalScore = apiResponse.Session?.TotalScore ?? 0,
                SuspiciousTime = apiResponse.Session?.SuspiciousTime ?? 0,
                Cheating = apiResponse.Cheating,
                Timestamp = DateTime.UtcNow,
                EvidenceImagePath = evidencePath,

                // New Granular Data
                PhoneDetected = apiResponse.PhoneDetection?.Detected ?? false,
                PhoneConfidence = apiResponse.PhoneDetection?.Confidence ?? 0,
                PersonCount = apiResponse.PersonDetection?.PersonCount ?? 0,
                EyeStatus = apiResponse.EyeTracking?.Status,
                HeadStatus = apiResponse.HeadPose?.Status,
                FacePresent = apiResponse.FaceDetection?.FacePresent ?? false
            };

            await _unitOfWork.Repository<ProctoringLog>().AddAsync(log);
            await _unitOfWork.CompleteAsync();

            _logger.LogDebug("Saved granular proctoring log for Student {StudentId}, Exam {ExamId}. Event: {Event}, Risk: {Risk}",
                request.StudentId, request.ExamId, log.CurrentEvent, log.RiskLevel);
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
