namespace Exam.Application.Dto.Proctoring
{
    public class ExamProctoringSummaryDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ExamId { get; set; }
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

    public class StudentViolationDetailDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public List<ViolationTimelineDto> Timeline { get; set; } = new();
        public int TotalViolations { get; set; }
        public string? FinalRiskLevel { get; set; }
    }

    public class ViolationTimelineDto
    {
        public DateTime Timestamp { get; set; }
        public string? Event { get; set; }
        public string? RiskLevel { get; set; }
        public double Score { get; set; }
        public string? DetailsJson { get; set; }
    }

    public class ExamAnalyticsDto
    {
        public int TotalStudents { get; set; }
        public int HighRiskCount { get; set; }
        public int CriticalCount { get; set; }
        public double AverageScore { get; set; }
        public string? MostCommonViolation { get; set; }
        public Dictionary<string, int> ViolationDistribution { get; set; } = new();
        public Dictionary<string, int> RiskLevelDistribution { get; set; } = new();
    }
}
