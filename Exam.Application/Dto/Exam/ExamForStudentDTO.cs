using Exam.Application.Dto.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Exam
{
    public class ExamForStudentDTO
    {
        public int Id { get; set; }
        // معرف الامتحان

        public string Name { get; set; } = string.Empty;
        // اسم الامتحان

        public DateTime StartDate { get; set; }
        // وقت بدء الامتحان

        public DateTime DueDate { get; set; }
        // وقت الإغلاق

        public int DurationMinutes { get; set; }
        // مدة الامتحان بالدقائق

        public List<QuestionForExamDTO> Questions { get; set; } = new();
        // الأسئلة التي سيحلها الطالب
    }
}
