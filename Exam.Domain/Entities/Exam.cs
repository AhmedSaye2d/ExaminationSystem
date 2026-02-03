using Exam.Domain.Entities.Common;
using Exam.Domain.Entities.Identity;
using Exam.Domain.Enum;
using System;
using System.Collections.Generic;

namespace Exam.Domain.Entities
{
    public class Exam : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int PassScore { get; set; }
        public int TotalScore { get; set; }
        public ExamStatus Status { get; set; } = ExamStatus.Draft;
        public string? Instructions { get; set; }
        public bool ShuffleQuestions { get; set; } = false;
        public bool ShuffleOptions { get; set; } = false;

        // Relationships
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public string InstructorId { get; set; } = string.Empty;
        public AppUser Instructor { get; set; } = null!;

        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
