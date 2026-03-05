using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Exam
{
    public class ExamReadDTO
    {
        public int Id { get; set; }
        // معرف الامتحان

        public string Name { get; set; } = string.Empty;
        // اسم الامتحان

        public DateTime StartDate { get; set; }
        // وقت بدء الامتحان

        public DateTime DueDate { get; set; }
        // وقت انتهاء الامتحان

        public int TotalQuestions { get; set; }
        // عدد الأسئلة

        public int TotalPoints { get; set; }
        // الدرجة الكلية
    }
}
