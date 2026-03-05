namespace Exam.Application.Services.Interfaces.IExamStudentServices
{
    public interface IStudentExamService
    {
        Task<int> StartExamAsync(int examId, int studentId);

        Task SaveAnswerAsync(int examStudentId, int questionId, int choiceId);

        Task SubmitExamAsync(int examStudentId);
    }
}