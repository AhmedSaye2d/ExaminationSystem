using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Choice
{
    public class ChoiceUpdateDTO
    {
        public int Id { get; set; }
        // معرف الاختيار المراد تعديله

        public string Text { get; set; } = string.Empty;
        // نص الاختيار بعد التعديل

        public bool IsCorrect { get; set; }
        // تحديث حالة الإجابة الصحيحة
    }
}
