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
        public int TotalPoints { get; set; }
        public ExamType Type { get; set; }

        public int CourseID { get; set; }
        public Course Course { get; set; }

        public int InstructorID { get; set; }
        public Instructor Instructor { get; set; }

        public ExamSettings Settings { get; set; }
        public HashSet<ExamQuestion> ExamQuestions { get; set; }
        public List<ExamStudent> ExamStudents { get; set; }
        public int TotalGrade { get; set; }
    }
}
