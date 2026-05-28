namespace Exam.Application.Dto.SubmitExam
{
    public class StudentExamAnswerResponseDTO
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int? SelectedChoiceId { get; set; }
        public string SelectedChoiceText { get; set; }
    }
}
