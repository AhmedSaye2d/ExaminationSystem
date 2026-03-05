using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Domain.Entities
{
    public class ExamSettings
    {
       
            public bool ShuffleQuestions { get; set; }
            // هل يتم خلط ترتيب الأسئلة عشوائياً لكل طالب
            // True = كل طالب يشوف الأسئلة بترتيب مختلف (يقلل الغش)
            // False = نفس ترتيب الأسئلة لكل الطلاب

            public bool ShuffleChoices { get; set; }
            // هل يتم خلط الاختيارات داخل كل سؤال
            // يمنع أن تكون الإجابة الصحيحة دائمًا في نفس المكان (مثلاً الاختيار الأول)

            public int DurationMinutes { get; set; }
            // مدة الامتحان بالدقائق
            // بعد انتهاء الوقت يتم غلق الامتحان تلقائياً أو تسليمه تلقائياً

            public bool ShowResultAfterSubmit { get; set; }
        // هل تظهر نتيجة الطالب مباشرة بعد تسليم الامتحان
        // True = الطالب يرى الدرجة فوراً
        // False = النتيجة تظهر لاحقاً بعد مراجعة المدرس
        public bool AllowReview { get; set; }
    }

}
