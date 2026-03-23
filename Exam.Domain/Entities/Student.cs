using Exam.Domain.Entities.Identity;
using Exam.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Domain.Entities
{
    public class Student : AppUser
    {
        // Major (Department)
        public int MajorId { get; set; }//لقسم اللي الطالب تابع ليه
        public Department Major { get; set; }

        // Courses the student is enrolled in
        public List<CourseStudent> CourseStudents { get; set; } = new();

        // Exams taken by the student
        public List<ExamStudent> ExamStudents { get; set; } = new();
    }

}
