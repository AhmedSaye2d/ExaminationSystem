namespace Exam.Application.Services.Interfaces.IExamStudentServices
{
    public interface IStudentExamService
    {
        Task<Exam.Application.Dto.SubmitExam.StartExamResponseDTO> StartExamAsync(int examId, int studentId);

        Task SaveAnswerAsync(int examStudentId, int studentId, int questionId, int choiceId);

        Task<Exam.Application.Dto.SubmitExam.ExamResultDTO> SubmitExamAsync(int examStudentId, int studentId, IEnumerable<Exam.Application.Dto.SubmitExam.ExamAnswerDTO>? answers = null);

        Task<Exam.Application.Dto.SubmitExam.ExamResultDTO> GetExamResultAsync(int examId, int studentId);
        Task<Exam.Application.Dto.SubmitExam.ExamResultDTO> GetResultBySessionAsync(int examStudentId, int studentId);
        Task<IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO>> GetStudentResultsAsync(int studentId);
        Task<(IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO> Items, int TotalCount)> GetStudentResultsPagedAsync(int studentId, int page, int pageSize);
        Task<IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO>> GetExamResultsAsync(int examId);
        Task<(IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO> Items, int TotalCount)> GetExamResultsPagedAsync(int examId, int page, int pageSize);

        Task<IEnumerable<Exam.Application.Dto.Question.QuestionForStudentDTO>> GetExamQuestionsAsync(int examStudentId, int studentId);

        // Resume an in-progress exam session
        Task<Exam.Application.Dto.SubmitExam.ResumeExamDTO> ResumeExamAsync(int examStudentId, int studentId);

        Task<IEnumerable<Exam.Application.Dto.SubmitExam.StudentExamAnswerResponseDTO>> GetStudentAnswersAsync(int examStudentId, int studentId);

        Task<IEnumerable<Exam.Application.Dto.Question.QuestionReadDTO>> GetExamSolutionsAsync(int examId, int studentId);
        Task<IEnumerable<Exam.Application.Dto.SubmitExam.StudentExamAnswerResponseDTO>> GetMyAnswersByExamIdAsync(int examId, int studentId);
    }
}