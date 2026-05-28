using Exam.Application.Dto.Choice;
using Exam.Domain.Enum;

namespace Exam.Application.Dto.Question
{
    public class QuestionDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Grade { get; set; }
        public QuestionType Type { get; set; }
        public List<ChoiceDTO> Choices { get; set; } = new();
    }
}
