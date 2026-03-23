namespace Exam.Application.Dto.Exam
{
    public class AssignQuestionDTO
    {
        public int QuestionId { get; set; }
        public int Points { get; set; }
        // Order property is not mapped currently to Question, but requested by user
        public int Order { get; set; } 
    }
}
