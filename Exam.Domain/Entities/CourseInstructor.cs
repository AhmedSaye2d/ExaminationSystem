using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
