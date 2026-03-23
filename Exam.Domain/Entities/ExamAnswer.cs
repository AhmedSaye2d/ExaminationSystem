using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Domain.Entities
{
    public class ExamAnswer : BaseEntity
    {
        public int ExamStudentId { get; set; }
        public ExamStudent? ExamStudent { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int ExamId { get; set; }
        public Exam? Exam { get; set; }

        public int QuestionId { get; set; }
        public Question? Question { get; set; }

        public int ChoiceId { get; set; } // nullable لو سؤال مقالي
        public Choice? Choice { get; set; }

        public string? WrittenAnswer { get; set; } // Essay support
    }
}