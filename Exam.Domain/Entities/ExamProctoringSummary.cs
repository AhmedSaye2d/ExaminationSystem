using Exam.Domain.Entities.Common;

namespace Exam.Domain.Entities
{
    public class ExamProctoringSummary : BaseEntity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public string StudentName { get; set; } = string.Empty;

        public int ExamId { get; set; }
        public Exam Exam { get; set; } = null!;
        public string ExamTitle { get; set; } = string.Empty;

        public string? FinalRiskLevel { get; set; }
        public double FinalScore { get; set; }
        public double SuspiciousTime { get; set; }

        public int PhoneDetectionCount { get; set; }
        public int ExtraPersonCount { get; set; }
        public int NoFaceCount { get; set; }
        public int HeadViolationCount { get; set; }
        public int GazeViolationCount { get; set; }
        public int LongGapCount { get; set; }
        public int TotalViolations { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsFlagged { get; set; }
    }
}
