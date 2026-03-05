using Exam.Application.Dto.Exam;

namespace Exam.Application.Services.Interfaces.IExamServices
{
    public interface IExamService
    {
        Task<IEnumerable<ExamDTO>> GetAllAsync();
        Task<ExamDTO> GetByIdAsync(int id);

        Task CreateAsync(ExamCreateDTO dto);
        Task UpdateAsync(int id, ExamCreateDTO dto);
        Task DeleteAsync(int id);

        Task AddQuestionsToExamAsync(int examId, IEnumerable<int> questionIds);
    }
}