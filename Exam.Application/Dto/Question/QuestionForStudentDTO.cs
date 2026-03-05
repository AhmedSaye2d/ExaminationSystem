using Exam.Domain.Enum;

namespace Exam.Application.Dto.Question
{
    public class QuestionForStudentDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Grade { get; set; }
        public QuestionType Type { get; set; }
        public List<ChoiceForStudentDTO> Choices { get; set; } = new();
    }

    public class ChoiceForStudentDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
