
using Exam.Application.Dto.Course;
using Exam.Domain.Enum;

namespace Exam.Application.Dto.Student
{
    public class StudentDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public double GPA { get; set; }
        public int MajorId { get; set; }
        public List<CourseDTO> EnrolledCourses { get; set; } = new();
    }
}
