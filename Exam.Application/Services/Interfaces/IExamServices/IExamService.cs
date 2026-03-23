using Exam.Application.Dto.Exam;

namespace Exam.Application.Services.Interfaces.IExamServices
{
    public interface IExamService
    {
        Task<IEnumerable<ExamDTO>> GetAllAsync();
        Task<(IEnumerable<ExamDTO> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? courseId = null);
        Task<IEnumerable<ExamDTO>> GetInstructorExamsAsync(int instructorId);
        Task<ExamDTO> GetByIdAsync(int id);
        Task<ExamStatsDTO> GetExamStatsAsync(int examId, int instructorId);

        Task CreateAsync(ExamCreateDTO dto);
        Task UpdateAsync(int id, ExamCreateDTO dto, int instructorId);
        Task DeleteAsync(int id, int instructorId);

        // Manage questions in an exam
        Task<IEnumerable<Exam.Application.Dto.Question.QuestionDTO>> GetQuestionsByExamIdAsync(int examId, int instructorId);
        Task AddQuestionToExamAsync(int examId, int questionId, int points, int order, int instructorId);
        Task RemoveQuestionFromExamAsync(int examId, int questionId, int instructorId);

        // Schedule and publish
        Task ScheduleExamAsync(int id, ScheduleExamDTO dto, int instructorId);
        Task PublishExamAsync(int id, int instructorId);
        Task UnpublishExamAsync(int id, int instructorId);

        // Instructor-facing results for a specific exam
        Task<IEnumerable<Exam.Application.Dto.Exam.InstructorExamResultDTO>> GetExamResultsForInstructorAsync(int examId, int instructorId);
    }
}