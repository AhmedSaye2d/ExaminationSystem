using Exam.Application.Dto.Question;

namespace Exam.Application.Dto.SubmitExam
{
    public class StartExamResponseDTO
    {
        public int ExamStudentId { get; set; }
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalGrade { get; set; }
        public int PassingScore { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ServerTime { get; set; }
        public int RemainingSeconds { get; set; }
        public string Status { get; set; } = "active";
        public IEnumerable<QuestionForStudentDTO> Questions { get; set; } = new List<QuestionForStudentDTO>();
    }
}
