using Exam.Application.Dto.Proctoring;

namespace Exam.Application.Services.Interfaces
{
    public interface IReportingService
    {
        Task<IEnumerable<ProctoringReportDto>> GetReportsByExamIdAsync(int examId);
        Task<byte[]> GetReportsExcelByExamIdAsync(int examId, string baseUrl = "https://localhost:7236");
        Task<string> GetStudentSessionReportTxtAsync(int examId, int studentId, string baseUrl = "https://localhost:7236");
        Task<byte[]> GetStudentSessionReportExcelAsync(int examId, int studentId, string baseUrl = "https://localhost:7236");
        Task<IEnumerable<SessionReportDto>> GetSessionReportsByExamIdAsync(int examId);
    }
}