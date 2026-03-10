using Exam.Application.Dto.Exam;

namespace Exam.Application.Services.Interfaces.IExamServices
{
    public interface IExamService
    {
        Task<IEnumerable<ExamDTO>> GetAllAsync();
        Task<IEnumerable<ExamDTO>> GetInstructorExamsAsync(int instructorId);
        Task<ExamDTO> GetByIdAsync(int id);
        Task<ExamStatsDTO> GetExamStatsAsync(int examId, int instructorId);

        Task CreateAsync(ExamCreateDTO dto);
        Task UpdateAsync(int id, ExamCreateDTO dto, int instructorId);
        Task DeleteAsync(int id, int instructorId);
    }
}