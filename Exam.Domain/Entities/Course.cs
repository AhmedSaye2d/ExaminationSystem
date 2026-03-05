using Exam.Domain.Common;
using Exam.Domain.Entities.Common;

namespace Exam.Domain.Entities
{
  
        public class Course : BaseEntity
    {
            // Basic Info
            public string Name { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public string? Description { get; set; }

            // Course Duration
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }

            // Academic Info
            public int CreditHours { get; set; }

            // Department Relation
            public int DepartmentId { get; set; }
            public Department Department { get; set; }

            // Instructors (Many-to-Many)
            public List<CourseInstructor> CourseInstructors { get; set; } = new();

            // Students (Many-to-Many)
            public List<CourseStudent> CourseStudents { get; set; } = new();

            // Exams in this course
            public HashSet<Exam> Exams { get; set; } = new();
        }

    }

