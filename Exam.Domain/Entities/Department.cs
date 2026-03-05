using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Domain.Entities
{
    public class Department : BaseEntity
    {
        // Basic Info
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Relations
        public List<Course> Courses { get; set; } = new();
        public List<Instructor> Instructors { get; set; } = new();
        public List<Student> Students { get; set; } = new();
    }
}
