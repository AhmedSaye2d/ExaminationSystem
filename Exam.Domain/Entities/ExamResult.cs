using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
using Exam.Domain.Enum;
namespace Exam.Domain.Entities
{
    public class ExamResult : BaseEntity
    {
        public int ExamStudentId { get; set; }
        public ExamStudent? ExamStudent { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int ExamId { get; set; }
        public Exam? Exam { get; set; }

        public double Score { get; set; }
        public double Total { get; set; }
        public bool IsPassed { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }
}
