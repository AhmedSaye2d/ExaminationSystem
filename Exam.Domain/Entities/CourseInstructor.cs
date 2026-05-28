using Exam.Domain.Entities.Common;

namespace Exam.Domain.Entities
{
    public class CourseInstructor : BaseEntity
    {

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public int InstructorId { get; set; }
        public Instructor? Instructor { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }

}
