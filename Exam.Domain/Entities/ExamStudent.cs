using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exam.Domain.Enum;

namespace Exam.Domain.Entities
{
    public class ExamStudent : BaseEntity
    {
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int ExamId { get; set; }
        public Exam? Exam { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ExamStatus Status { get; set; } = ExamStatus.NotStarted;

        public double Score { get; set; }
        public bool IsPassed { get; set; }

        public List<ExamAnswer> ExamAnswers { get; set; } = new();
    }
}
