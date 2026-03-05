using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Domain.Entities
{
    public class ExamStudent : BaseEntity
    // يمثل محاولة الطالب للامتحان + النتيجة النهائية

    {
        public int StudentId { get; set; }
        // الطالب الذي يؤدي الامتحان

        public Student? Student { get; set; }
        // بيانات الطالب

        public int ExamId { get; set; }
        // الامتحان الذي يتم حله

        public Exam? Exam { get; set; }
        // بيانات الامتحان

        public DateTime StartDate { get; set; }
        // وقت بدء الامتحان من قبل الطالب
        // يستخدم لحساب الوقت المتبقي

        public DateTime? SubmissionDate { get; set; }
        // وقت تسليم الامتحان
        // يكون فارغ إذا لم يتم التسليم

        public bool IsSubmitted { get; set; } = false;
        // هل الطالب قام بتسليم الامتحان أم لا

        public double Score { get; set; }
        // الدرجة النهائية بعد التصحيح التلقائي

        public bool IsPassed { get; set; }
        // حالة النجاح أو الرسوب بناءً على Passing Score

        public List<ExamAnswer> ExamAnswers { get; set; } = new();
        // جميع إجابات الطالب داخل الامتحان
    }

}
