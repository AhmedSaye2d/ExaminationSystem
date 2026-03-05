using Exam.Application.Dto.Question;

namespace Exam.Application.Services.Interfaces.IQuestionServices
{
    public interface IQuestionService
    {
        Task<IEnumerable<QuestionDTO>> GetAllAsync();
        Task<(IEnumerable<QuestionDTO> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? courseId);
        Task<QuestionDTO> GetByIdAsync(int id);

        Task CreateAsync(QuestionCreateDTO dto);
        Task UpdateAsync(int id, QuestionCreateDTO dto);
        Task DeleteAsync(int id);

        Task<int> AddQuestionWithChoicesAsync(QuestionWithChoicesDTO dto);
    }
}