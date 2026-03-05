using Exam.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Domain.Entities
{
    public class Instructor : AppUser
    {

        // Department
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        // Courses the instructor teaches (Many-to-Many)
        public List<CourseInstructor> CourseInstructors { get; set; } = new();
        public DateTime? HireDate { get; set; }

        // Exams created by this instructor
        public HashSet<Exam> Exams { get; set; } = new();
    }

}
