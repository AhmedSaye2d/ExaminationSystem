using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Course
{
    public class CourseCreateDTO
    {
        public string Name { get; set; } = string.Empty;
        // اسم الكورس

        public string Code { get; set; } = string.Empty;
        // كود الكورس

        public string? Description { get; set; }
        // وصف الكورس

        public DateTime StartDate { get; set; }
        // بداية الكورس

        public DateTime EndDate { get; set; }
        // نهاية الكورس

        public int CreditHours { get; set; }
        // عدد الساعات المعتمدة

        public int DepartmentId { get; set; }
        // القسم التابع له الكورس
    }
}
