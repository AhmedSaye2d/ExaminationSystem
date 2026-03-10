namespace Exam.Application.Dto.Exam
{
    public class ExamStatsDTO
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int SubmittedCount { get; set; }
        public double AverageScore { get; set; }
    }
}
