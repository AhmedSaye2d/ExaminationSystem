namespace Exam.Application.Services.Interfaces.IExamStudentServices
{
    public interface IStudentExamService
    {
        Task<int> StartExamAsync(int examId, int studentId);

        Task SaveAnswerAsync(int examStudentId, int questionId, int choiceId);

        Task SubmitExamAsync(int examStudentId);

        Task<Exam.Application.Dto.SubmitExam.ExamResultDTO> GetExamResultAsync(int examId, int studentId);
        Task<Exam.Application.Dto.SubmitExam.ExamResultDTO> GetResultBySessionAsync(int examStudentId);
        Task<IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO>> GetStudentResultsAsync(int studentId);
        Task<IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO>> GetExamResultsAsync(int examId);

        Task<IEnumerable<Exam.Application.Dto.Question.QuestionForStudentDTO>> GetExamQuestionsAsync(int examStudentId);
    }
}