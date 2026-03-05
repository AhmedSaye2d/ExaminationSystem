using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Exam
{
    public class ExamCreateDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int CourseId { get; set; }
        public int InstructorId { get; set; }

        public global::Exam.Domain.Entities.ExamSettings? Settings { get; set; }
    }
}
