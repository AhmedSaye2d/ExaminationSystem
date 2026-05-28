namespace Exam.Application.Dto.SubmitExam
{
    public class SubmitExamDTO
    {
        public int ExamId { get; set; }
        // معرف الامتحان

        public List<StudentAnswerDTO> Answers { get; set; } = new();
        // إجابات الطالب على كل الأسئلة
    }
}
