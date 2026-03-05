using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Question
{
    public class QuestionUpdateDTO
    {
        public int Id { get; set; }
        // معرف السؤال

        public string Text { get; set; } = string.Empty;
        // نص السؤال بعد التعديل

        public int Grade { get; set; }
        // تعديل درجة السؤال

        public int Type { get; set; }
        // نوع السؤال
    }
}
