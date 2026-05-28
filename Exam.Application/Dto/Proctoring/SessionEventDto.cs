namespace Exam.Application.Dto.Proctoring
{
    public class SessionEventDto
    {
        public double Second { get; set; }
        public string EventName { get; set; } = string.Empty;
        public double DurationSeconds { get; set; }
        public string? EvidenceImagePath { get; set; }
        public double CheatScore { get; set; }
        public string? RiskLevel { get; set; }
    }
}
