using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Domain.Entities
{
    public class CourseStudent : BaseEntity
    {
        // Course
        public int CourseId { get; set; }
        public Course Course { get; set; }

        // Student
        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}
