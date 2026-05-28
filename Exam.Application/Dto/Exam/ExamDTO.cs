using Exam.Domain.Enum;

namespace Exam.Application.Dto.Exam
{
    public class ExamDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public ExamType Type { get; set; }
        public int CourseId { get; set; }
        public int InstructorId { get; set; }
        public int TotalGrade { get; set; }
        public int PassingScore { get; set; }
    }

}
