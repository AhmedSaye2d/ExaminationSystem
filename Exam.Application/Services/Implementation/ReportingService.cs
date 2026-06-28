using AutoMapper;
using ClosedXML.Excel;
using Exam.Application.Dto.Proctoring;
using Exam.Application.Services.Interfaces;
using Exam.Domain.Entities;
using Exam.Domain.Interface;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Exam.Application.Services.Implementation
{
    public class ReportingService : IReportingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReportingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProctoringReportDto>> GetReportsByExamIdAsync(int examId)
        {
            var logs = await _unitOfWork.Repository<ProctoringLog>()
                .FindAsync(r => r.ExamId == examId && !r.IsDeleted, "Student");

            var orderedLogs = logs.OrderByDescending(l => l.Timestamp);
            return _mapper.Map<IEnumerable<ProctoringReportDto>>(orderedLogs);
        }

        public async Task<string> GetStudentSessionReportTxtAsync(int examId, int studentId, string baseUrl = "https://localhost:7236")
        {
            var examStudent = (await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(es => es.ExamId == examId && es.StudentId == studentId, "Student")).FirstOrDefault();

            if (examStudent == null) return "Session not found.";

            var logs = await _unitOfWork.Repository<ProctoringLog>()
                .FindAsync(l => l.ExamId == examId && l.StudentId == studentId && !l.IsDeleted);

            var report = CreateSessionReport(examStudent, logs.OrderBy(l => l.Timestamp).ToList());

            var sb = new StringBuilder();
            sb.AppendLine("=======================================================");
            sb.AppendLine("                    SESSION REPORT");
            sb.AppendLine("=======================================================");
            sb.AppendLine();
            sb.AppendLine($"Session ID         : {report.SessionId}");
            sb.AppendLine($"Student Name       : {report.StudentName}");
            sb.AppendLine($"Exam ID            : {report.ExamId}");
            sb.AppendLine();
            sb.AppendLine($"Session Start      : {report.SessionStart:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Session End        : {report.SessionEnd:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine($"Total Session Time : {report.TotalSessionTimeSeconds:F1}s");
            sb.AppendLine($"Suspicious Time    : {report.SuspiciousTimeSeconds:F1}s");
            sb.AppendLine();
            sb.AppendLine($"Total Cheat Score  : {report.TotalCheatScore:F1}");
            sb.AppendLine($"Final Risk Level   : {report.RiskLevel.ToUpper()}");
            sb.AppendLine();
            sb.AppendLine($"Cheating Detected  : {report.CheatingDetected.ToString().ToUpper()}");
            sb.AppendLine();
            sb.AppendLine("-------------------------------------------------------");
            sb.AppendLine(" Second        Event                Duration");
            sb.AppendLine("-------------------------------------------------------");
            sb.AppendLine();
            foreach (var ev in report.Events)
            {
                var displayName = ev.EventName;
                if (displayName == "Phone") displayName = "Phone Detected";
                string durationStr = $"{ev.DurationSeconds:F1}s";
                sb.AppendLine($"{ev.Second:F1}s".PadRight(14) + $"{displayName}".PadRight(21) + durationStr);
            }
            sb.AppendLine();
            sb.AppendLine("-------------------------------------------------------");
            sb.AppendLine();
            var evidenceImages = report.Events.Where(e => !string.IsNullOrEmpty(e.EvidenceImagePath)).Select(e => e.EvidenceImagePath).Distinct();
            if (evidenceImages.Any())
            {
                sb.AppendLine("Evidence Image Path:");
                foreach (var path in evidenceImages)
                {
                    sb.AppendLine($"{baseUrl.TrimEnd('/')}{path}");
                }
            }
            sb.AppendLine();
            sb.AppendLine("=======================================================");

            return sb.ToString();
        }

        public async Task<IEnumerable<SessionReportDto>> GetSessionReportsByExamIdAsync(int examId)
        {
            var sessions = new List<SessionReportDto>();

            var examStudents = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(es => es.ExamId == examId, "Student");

            var logs = await _unitOfWork.Repository<ProctoringLog>()
                .FindAsync(l => l.ExamId == examId && !l.IsDeleted);

            foreach (var es in examStudents)
            {
                var studentLogs = logs.Where(l => l.StudentId == es.StudentId).OrderBy(l => l.Timestamp).ToList();
                var report = CreateSessionReport(es, studentLogs);
                sessions.Add(report);
            }

            return sessions;
        }

        private SessionReportDto CreateSessionReport(ExamStudent es, List<ProctoringLog> logs)
        {
            // ── 1. Determine a reliable SessionStart ───────────────────────────────
            // We always derive SessionStart from the FIRST log timestamp.
            // This avoids issues where es.StartDate uses a different clock or was
            // never updated (e.g. DateTime.MinValue / stale row).
            // If there are no logs at all, fall back to es.StartDate.
            DateTime sessionStart;
            DateTime sessionEnd;

            if (logs.Any())
            {
                sessionStart = logs.First().Timestamp;
                sessionEnd   = logs.Last().Timestamp;

                // Sanity-check: if es.StartDate is close to the first log (within 5 min),
                // prefer es.StartDate as it is set at the moment the student clicks "Start".
                if (es.StartDate > DateTime.MinValue && Math.Abs((es.StartDate - sessionStart).TotalMinutes) < 5)
                    sessionStart = es.StartDate;
            }
            else
            {
                sessionStart = es.StartDate;
                sessionEnd   = es.EndDate ?? es.SubmissionDate ?? DateTime.UtcNow;
            }

            // Ensure sessionEnd >= sessionStart
            if (sessionEnd < sessionStart) sessionEnd = sessionStart;

            _logger.LogDebug(
                "Report [{ExamId}/{StudentId}] SessionStart={Start:u}, SessionEnd={End:u}, LogCount={Count}",
                es.ExamId, es.StudentId, sessionStart, sessionEnd, logs.Count);

            var report = new SessionReportDto
            {
                SessionId  = es.Id.ToString("x8"),
                StudentName = es.Student?.FullName ?? "Unknown",
                StudentId  = es.StudentId,
                ExamId     = es.ExamId,
                SessionStart = sessionStart,
                SessionEnd   = sessionEnd,
                Events = new List<SessionEventDto>()
            };

            report.TotalSessionTimeSeconds = (report.SessionEnd - report.SessionStart).TotalSeconds;
            if (report.TotalSessionTimeSeconds < 0) report.TotalSessionTimeSeconds = 0;

            var lastLog = logs.LastOrDefault();
            report.SuspiciousTimeSeconds = lastLog?.SuspiciousTime ?? 0;

            report.TotalCheatScore  = lastLog?.TotalScore ?? 0;
            report.RiskLevel        = lastLog?.RiskLevel ?? "LOW RISK";
            report.CheatingDetected = logs.Any(l => l.Cheating);

            // ── 2. Build event rows ────────────────────────────────────────────────
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var log in logs.Where(l => l.Cheating))
            {
                // Clamp offset: must be 0 ≤ second ≤ totalSession
                double offsetSeconds = (log.Timestamp - sessionStart).TotalSeconds;
                offsetSeconds = Math.Max(0, Math.Min(offsetSeconds, report.TotalSessionTimeSeconds));

                bool eventsArrayProcessed = false;

                if (!string.IsNullOrEmpty(log.EventsJson))
                {
                    FastApiResponseDto? responseDto = null;
                    try { responseDto = JsonSerializer.Deserialize<FastApiResponseDto>(log.EventsJson, jsonOptions); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to deserialize EventsJson — StudentId={StudentId}, ExamId={ExamId}, Timestamp={Timestamp}",
                            log.StudentId, log.ExamId, log.Timestamp);
                    }

                    if (responseDto?.Events != null && responseDto.Events.Any())
                    {
                        foreach (var evt in responseDto.Events)
                        {
                            double duration = ExtractDuration(evt, responseDto, log, sessionStart, logs);
                            report.Events.Add(new SessionEventDto
                            {
                                Second            = offsetSeconds,
                                EventName         = FormatEventName(evt),
                                DurationSeconds   = duration,
                                EvidenceImagePath = log.EvidenceImagePath,
                                CheatScore        = log.TotalScore,
                                RiskLevel         = log.RiskLevel,
                                Timestamp         = log.Timestamp
                            });
                        }
                        eventsArrayProcessed = true;
                    }
                }

                if (!eventsArrayProcessed)
                {
                    FastApiResponseDto? responseDto = null;
                    if (!string.IsNullOrEmpty(log.EventsJson))
                    {
                        try { responseDto = JsonSerializer.Deserialize<FastApiResponseDto>(log.EventsJson, jsonOptions); }
                        catch { /* already logged */ }
                    }

                    double duration = ExtractDuration(log.CurrentEvent, responseDto, log, sessionStart, logs);
                    report.Events.Add(new SessionEventDto
                    {
                        Second            = offsetSeconds,
                        EventName         = FormatEventName(log.CurrentEvent),
                        DurationSeconds   = duration,
                        EvidenceImagePath = log.EvidenceImagePath,
                        CheatScore        = log.TotalScore,
                        RiskLevel         = log.RiskLevel,
                        Timestamp         = log.Timestamp
                    });
                }
            }

            return report;
        }

        /// <summary>
        /// Extracts the real duration for a given event type from the FastApiResponseDto timers.
        /// For phone_detected with a 0.0 timer (instant confirmation), returns -1 as a sentinel
        /// so the report layer can display "Instant" instead of "0.0s".
        /// Falls back to inter-log delta when no timer value is available.
        /// </summary>
        private double ExtractDuration(
            string? evt,
            FastApiResponseDto? dto,
            ProctoringLog log,
            DateTime sessionStart,
            List<ProctoringLog> allLogs)
        {
            if (string.IsNullOrEmpty(evt)) return 0.0;

            double timer = evt switch
            {
                "phone_detected"  => dto?.PhoneDetection?.TimerSeconds  ?? -1,
                "extra_person"    => dto?.PersonDetection?.TimerSeconds  ?? -1,
                "no_face"         => dto?.FaceDetection?.TimerSeconds    ?? -1,
                "gaze_violation"  => dto?.EyeTracking?.TimerSeconds      ?? -1,
                "head_violation"  => dto?.HeadPose?.TimerSeconds         ?? -1,
                _                 => -1
            };

            // If a valid positive timer was found, use it.
            if (timer > 0) return timer;

            // Fallback: If no valid timer was found, return 0.0 (or -1 for instant).
            // Do not use the time elapsed since the session start as the event's duration!
            _logger.LogWarning(
                "Duration fallback used — StudentId={StudentId}, ExamId={ExamId}, Event={Event}, Timestamp={Timestamp}, Fallback set to 0.0s",
                log.StudentId, log.ExamId, evt, log.Timestamp);
                
            return 0.0;
        }

        private string FormatEventName(string? evt)
        {
            if (evt == null) return "Unknown";
            return evt switch
            {
                "no_face" => "No Face",
                "head_violation" => "Head Turn",
                "gaze_violation" => "Head And Gaze",
                "phone_detected" => "Phone",
                "extra_person" => "Extra Person",
                _ => evt
            };
        }

        public async Task<byte[]> GetReportsExcelByExamIdAsync(int examId, string baseUrl = "https://localhost:7236")
        {
            var sessions = (await GetSessionReportsByExamIdAsync(examId)).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Exam Report");

            // Consolidated Report Summary Info
            worksheet.Cell(1, 1).Value = "Exam ID";
            worksheet.Cell(1, 2).Value = examId;
            worksheet.Cell(2, 1).Value = "Total Students";
            worksheet.Cell(2, 2).Value = sessions.Count;
            worksheet.Cell(3, 1).Value = "Total Students With Violations";
            worksheet.Cell(3, 2).Value = sessions.Count(s => s.CheatingDetected);

            var currentRow = 6;

            // Data Header
            worksheet.Cell(currentRow, 1).Value = "Student Name";
            worksheet.Cell(currentRow, 2).Value = "Student ID";
            worksheet.Cell(currentRow, 3).Value = "Exam ID";
            worksheet.Cell(currentRow, 4).Value = "Event Type";
            worksheet.Cell(currentRow, 5).Value = "Duration";
            worksheet.Cell(currentRow, 6).Value = "Cheat Score";
            worksheet.Cell(currentRow, 7).Value = "Risk Level";
            worksheet.Cell(currentRow, 8).Value = "Timestamp";
            worksheet.Cell(currentRow, 9).Value = "Evidence Image Path";

            var headerRange = worksheet.Range(currentRow, 1, currentRow, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            currentRow++;

            foreach (var session in sessions)
            {
                if (session.Events != null && session.Events.Any())
                {
                    foreach (var ev in session.Events)
                    {
                        worksheet.Cell(currentRow, 1).Value = session.StudentName;
                        worksheet.Cell(currentRow, 2).Value = session.StudentId;
                        worksheet.Cell(currentRow, 3).Value = session.ExamId;
                        worksheet.Cell(currentRow, 4).Value = ev.EventName;
                        string durationStr = $"{ev.DurationSeconds:F1}s";
                        worksheet.Cell(currentRow, 5).Value = durationStr;
                        worksheet.Cell(currentRow, 6).Value = ev.CheatScore;
                        worksheet.Cell(currentRow, 7).Value = ev.RiskLevel;
                        worksheet.Cell(currentRow, 8).Value = ev.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                        if (!string.IsNullOrEmpty(ev.EvidenceImagePath))
                        {
                            var imageUrl = $"{baseUrl.TrimEnd('/')}{ev.EvidenceImagePath}";
                            worksheet.Cell(currentRow, 9).Value = ev.EvidenceImagePath;
                            worksheet.Cell(currentRow, 9).SetHyperlink(new XLHyperlink(imageUrl));
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 9).Value = "N/A";
                        }

                        currentRow++;
                    }
                }
            }

            if (!sessions.Any())
            {
                worksheet.Cell(currentRow, 1).Value = "No students found for this exam.";
                worksheet.Range(currentRow, 1, currentRow, 9).Merge();
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GetStudentSessionReportExcelAsync(int examId, int studentId, string baseUrl = "https://localhost:7236")
        {
            var sessions = (await GetSessionReportsByExamIdAsync(examId)).Where(s => s.StudentId == studentId).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Student Session Report");

            var currentRow = 1;

            // Data Header
            worksheet.Cell(currentRow, 1).Value = "Student Name";
            worksheet.Cell(currentRow, 2).Value = "Student ID";
            worksheet.Cell(currentRow, 3).Value = "Exam ID";
            worksheet.Cell(currentRow, 4).Value = "Event Type";
            worksheet.Cell(currentRow, 5).Value = "Duration";
            worksheet.Cell(currentRow, 6).Value = "Cheat Score";
            worksheet.Cell(currentRow, 7).Value = "Risk Level";
            worksheet.Cell(currentRow, 8).Value = "Timestamp";
            worksheet.Cell(currentRow, 9).Value = "Evidence Image Path";

            var headerRange = worksheet.Range(currentRow, 1, currentRow, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            currentRow++;

            var session = sessions.FirstOrDefault();
            if (session != null)
            {
                if (session.Events != null && session.Events.Any())
                {
                    foreach (var ev in session.Events)
                    {
                        worksheet.Cell(currentRow, 1).Value = session.StudentName;
                        worksheet.Cell(currentRow, 2).Value = session.StudentId;
                        worksheet.Cell(currentRow, 3).Value = session.ExamId;
                        worksheet.Cell(currentRow, 4).Value = ev.EventName;
                        string durationStr = $"{ev.DurationSeconds:F1}s";
                        worksheet.Cell(currentRow, 5).Value = durationStr;
                        worksheet.Cell(currentRow, 6).Value = ev.CheatScore;
                        worksheet.Cell(currentRow, 7).Value = ev.RiskLevel;
                        worksheet.Cell(currentRow, 8).Value = ev.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                        if (!string.IsNullOrEmpty(ev.EvidenceImagePath))
                        {
                            var imageUrl = $"{baseUrl.TrimEnd('/')}{ev.EvidenceImagePath}";
                            worksheet.Cell(currentRow, 9).Value = ev.EvidenceImagePath;
                            worksheet.Cell(currentRow, 9).SetHyperlink(new XLHyperlink(imageUrl));
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 9).Value = "N/A";
                        }

                        currentRow++;
                    }
                }
            }
            else
            {
                worksheet.Cell(currentRow, 1).Value = "Session not found.";
                worksheet.Range(currentRow, 1, currentRow, 9).Merge();
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}