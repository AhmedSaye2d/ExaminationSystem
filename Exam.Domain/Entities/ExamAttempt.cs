using Exam.Domain.Entities.Common;
using Exam.Domain.Entities.Identity;
using System;
using System.Collections.Generic;

namespace Exam.Domain.Entities
{
    public class ExamAttempt : BaseEntity
    {
        public string StudentId { get; set; } = string.Empty;
        public AppUser Student { get; set; } = null!;

        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!;

        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        
        public double Score { get; set; }
        public bool IsPassed { get; set; }

        public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }

    public class StudentAnswer : BaseEntity
    {
        public int ExamAttemptId { get; set; }
        public ExamAttempt ExamAttempt { get; set; } = null!;

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public int? SelectedOptionId { get; set; }
        public QuestionOption? SelectedOption { get; set; }

        public string? AnswerText { get; set; } // For ShortAnswer/Essay questions
        public bool IsCorrect { get; set; }
        public double MarksEarned { get; set; }
    }
}
