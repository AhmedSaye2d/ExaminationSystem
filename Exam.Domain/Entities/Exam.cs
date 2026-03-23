using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
using Exam.Domain.Enum;

namespace Exam.Domain.Entities
{
    public class Exam : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int TotalQuestions { get; set; }
        public ExamType Type { get; set; }

        public int CourseID { get; set; }
        public Course Course { get; set; }

        public int InstructorID { get; set; }
        public Instructor Instructor { get; set; }

        public ExamSettings Settings { get; set; }
        public List<Question> Questions { get; set; } = new();
        public List<ExamStudent> ExamStudents { get; set; }
        public int TotalGrade { get; set; }
        public int PassingScore { get; set; } = 0; // Minimum score to pass (set by instructor)
        public bool IsPublished { get; set; } = false;
    }
}
