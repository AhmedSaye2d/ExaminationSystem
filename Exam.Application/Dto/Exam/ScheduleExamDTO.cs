namespace Exam.Application.Dto.Exam
{
    public class ScheduleExamDTO
    {
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
    }
}
