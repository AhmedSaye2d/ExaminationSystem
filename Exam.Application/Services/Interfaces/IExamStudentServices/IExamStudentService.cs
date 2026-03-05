namespace Exam.Application.Services.Interfaces.IExamStudentServices
{
    public interface IStudentExamService
    {
        Task<int> StartExamAsync(int examId, int studentId);

        Task SaveAnswerAsync(int examStudentId, int examQuestionId, int choiceId);

        Task SubmitExamAsync(int examStudentId);

        Task<Exam.Application.Dto.SubmitExam.ExamResultDTO> GetExamResultAsync(int examId, int studentId);
        Task<IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO>> GetStudentResultsAsync(int studentId);
        Task<IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO>> GetExamResultsAsync(int examId);
    }
}