using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
namespace Exam.Domain.Entities
{
    public class ExamResult : BaseEntity
    {
        public int ExamStudentId { get; set; }
        // ربط النتيجة بمحاولة الطالب للامتحان

        public ExamStudent? ExamStudent { get; set; }
        // بيانات الطالب داخل الامتحان (Navigation Property)

        public double Score { get; set; }
        // الدرجة النهائية بعد التصحيح التلقائي

        public bool IsPassed { get; set; }
        // هل الطالب نجح أم رسب بناءً على Pass Mark
    }

}
