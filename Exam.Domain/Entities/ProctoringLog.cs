using Exam.Domain.Entities.Common;

namespace Exam.Domain.Entities
{
    public class ProctoringLog : BaseEntity
    {
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int ExamId { get; set; }
        public Exam? Exam { get; set; }

        public string? CurrentEvent { get; set; }
        public string? EventsJson { get; set; }
        public string? RiskLevel { get; set; }
        public double TotalScore { get; set; }
        public double SuspiciousTime { get; set; }
        public bool Cheating { get; set; }
        public DateTime Timestamp { get; set; }

        // Granular Detection Results
        public bool PhoneDetected { get; set; }
        public double PhoneConfidence { get; set; }
        public int PersonCount { get; set; }
        public string? EyeStatus { get; set; }
        public string? HeadStatus { get; set; }
        public bool FacePresent { get; set; }
        public string? EvidenceImagePath { get; set; }
    }
}
