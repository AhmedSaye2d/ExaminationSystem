namespace Exam.Application.Dto.Proctoring
{
    public class SessionReportDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public int ExamId { get; set; }

        public DateTime SessionStart { get; set; }
        public DateTime SessionEnd { get; set; }

        public double TotalSessionTimeSeconds { get; set; }
        public double SuspiciousTimeSeconds { get; set; }

        public double TotalCheatScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;

        public bool CheatingDetected { get; set; }

        public List<SessionEventDto> Events { get; set; } = new();
    }
}
