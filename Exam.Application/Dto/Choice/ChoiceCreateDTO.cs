using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Choice
{
    public class ChoiceCreateDTO
    {
        public string Text { get; set; } = string.Empty;
        // نص الاختيار الذي سيظهر للطالب في السؤال

        public bool IsCorrect { get; set; }
        // يحدد هل هذا الاختيار هو الإجابة الصحيحة أم لا

        public int QuestionId { get; set; }
        // معرف السؤال المرتبط بالاختيار
        // مهم لربط Choice بـ Question في قاعدة البيانات
    }
}
