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

        public async Task<string> GetStudentSessionReportTxtAsync(int examId, int studentId)
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
                sb.AppendLine($"{ev.Second:F1}s".PadRight(14) + $"{ev.EventName}".PadRight(21) + $"{ev.DurationSeconds:F1}s");
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
                    sb.AppendLine(path);
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
            var report = new SessionReportDto
            {
                SessionId = es.Id.ToString("x8"),
                StudentName = es.Student?.FullName ?? "Unknown",
                StudentId = es.StudentId,
                ExamId = es.ExamId,
                SessionStart = es.StartDate,
                SessionEnd = es.EndDate ?? es.SubmissionDate ?? DateTime.UtcNow,
                Events = new List<SessionEventDto>()
            };

            report.TotalSessionTimeSeconds = (report.SessionEnd - report.SessionStart).TotalSeconds;
            if (report.TotalSessionTimeSeconds < 0) report.TotalSessionTimeSeconds = 0;

            var lastLog = logs.LastOrDefault();
            // Determine if SuspiciousTime is incremental per log or cumulative in the last log.
            // If any log (except the last) has a non‑zero SuspiciousTime, we assume incremental values.
            // Otherwise we keep the latest cumulative value.
            if (logs.Count > 1 && logs.Take(logs.Count - 1).Any(l => l.SuspiciousTime > 0))
            {
                report.SuspiciousTimeSeconds = logs.Sum(x => x.SuspiciousTime);
            }
            else
            {
                report.SuspiciousTimeSeconds = lastLog?.SuspiciousTime ?? 0;
            }
            report.TotalCheatScore = lastLog?.TotalScore ?? 0;
            report.RiskLevel = lastLog?.RiskLevel ?? "LOW RISK";
            report.CheatingDetected = logs.Any(l => l.Cheating);



            foreach (var log in logs.Where(l => l.Cheating))
            {
                double duration = 2.0; // fallback if timer values are missing
                if (!string.IsNullOrEmpty(log.EventsJson))
                {
                    try
                    {
                        var responseDto = JsonSerializer.Deserialize<FastApiResponseDto>(log.EventsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (responseDto != null)
                        {
                            if (log.CurrentEvent == "phone_detected" && responseDto.PhoneDetection != null)
                                duration = responseDto.PhoneDetection.TimerSeconds;
                            else if (log.CurrentEvent == "extra_person" && responseDto.PersonDetection != null)
                                duration = responseDto.PersonDetection.TimerSeconds;
                            else if (log.CurrentEvent == "no_face" && responseDto.FaceDetection != null)
                                duration = responseDto.FaceDetection.TimerSeconds;
                            else if (log.CurrentEvent == "gaze_violation" && responseDto.EyeTracking != null)
                                duration = responseDto.EyeTracking.TimerSeconds;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log a warning with context information for debugging
                        _logger.LogWarning(ex, "Failed to deserialize EventsJson for ExamId {ExamId}, StudentId {StudentId}, Timestamp {Timestamp}", log.ExamId, log.StudentId, log.Timestamp);
                    }
                }

                // deltaScore is no longer used; removed for clarity.
                // If needed in the future, it can be added back to SessionEventDto and exported to Excel.

                report.Events.Add(new SessionEventDto
                {
                    Second = (log.Timestamp - report.SessionStart).TotalSeconds,
                    EventName = FormatEventName(log.CurrentEvent),
                    DurationSeconds = duration,
                    EvidenceImagePath = log.EvidenceImagePath,
                    CheatScore = log.TotalScore,
                    RiskLevel = log.RiskLevel
                });


            }

            return report;
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

        public async Task<byte[]> GetReportsExcelByExamIdAsync(int examId)
        {
            var sessions = (await GetSessionReportsByExamIdAsync(examId)).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Exam Report");

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
                        worksheet.Cell(currentRow, 5).Value = $"{ev.DurationSeconds:F1}s";
                        worksheet.Cell(currentRow, 6).Value = ev.CheatScore;
                        worksheet.Cell(currentRow, 7).Value = ev.RiskLevel;
                        worksheet.Cell(currentRow, 8).Value = $"{ev.Second:F1}s";
                        if (!string.IsNullOrEmpty(ev.EvidenceImagePath))
                        {
                            var imageUrl = $"https://localhost:7236{ev.EvidenceImagePath}";
                            worksheet.Cell(currentRow, 9).Value = "Open Evidence";
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

        public async Task<byte[]> GetStudentSessionReportExcelAsync(int examId, int studentId)
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
                        worksheet.Cell(currentRow, 5).Value = $"{ev.DurationSeconds:F1}s";
                        worksheet.Cell(currentRow, 6).Value = ev.CheatScore;
                        worksheet.Cell(currentRow, 7).Value = ev.RiskLevel;
                        worksheet.Cell(currentRow, 8).Value = $"{ev.Second:F1}s";
                        if (!string.IsNullOrEmpty(ev.EvidenceImagePath))
                        {
                            var imageUrl = $"https://localhost:7236{ev.EvidenceImagePath}";
                            worksheet.Cell(currentRow, 9).Value = "Open Evidence";
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