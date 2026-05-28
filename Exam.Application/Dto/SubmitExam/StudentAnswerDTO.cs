namespace Exam.Application.Dto.SubmitExam
{
    public class StudentAnswerDTO
    {
        public int QuestionId { get; set; }
        // معرف السؤال

        public int ChoiceId { get; set; }
        // الاختيار الذي حدده الطالب
    }
}
