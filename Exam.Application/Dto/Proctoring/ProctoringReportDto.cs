namespace Exam.Application.Dto.Proctoring
{
    public class ProctoringReportDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string? CurrentEvent { get; set; }
        public string? RiskLevel { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Cheating { get; set; }
        public bool PhoneDetected { get; set; }
        public double PhoneConfidence { get; set; }
        public int PersonCount { get; set; }
        public string? EyeStatus { get; set; }
        public string? HeadStatus { get; set; }
        public bool FacePresent { get; set; }
        public double SuspiciousTime { get; set; }
    }
}