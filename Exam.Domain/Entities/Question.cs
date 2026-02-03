using Exam.Domain.Entities.Common;
using Exam.Domain.Enum;
using System.Collections.Generic;

namespace Exam.Domain.Entities
{
    public class Question : BaseEntity
    {
        public string Header { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int Marks { get; set; }
        public QuestionType Type { get; set; }
        public string? ImageUrl { get; set; }

        // Relationships
        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!;

        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}
