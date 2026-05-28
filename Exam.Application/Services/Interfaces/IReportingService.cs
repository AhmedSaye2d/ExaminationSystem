using Exam.Application.Dto.Proctoring;

namespace Exam.Application.Services.Interfaces
{
    public interface IReportingService
    {
        Task<IEnumerable<ProctoringReportDto>> GetReportsByExamIdAsync(int examId);
        Task<byte[]> GetReportsExcelByExamIdAsync(int examId);
        Task<string> GetStudentSessionReportTxtAsync(int examId, int studentId);
        Task<byte[]> GetStudentSessionReportExcelAsync(int examId, int studentId);
        Task<IEnumerable<SessionReportDto>> GetSessionReportsByExamIdAsync(int examId);
    }
}