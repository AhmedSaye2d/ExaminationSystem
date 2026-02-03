using Exam.Domain.Entities.Common;

namespace Exam.Domain.Entities
{
    public class QuestionOption : BaseEntity
    {
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }

        // Relationships
        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
    }
}
