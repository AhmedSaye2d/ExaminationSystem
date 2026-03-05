using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
namespace Exam.Domain.Entities
{
    public class Choice : BaseEntity
    {
        public string Text { get; set; } = string.Empty;
        // نص الاختيار

        public bool IsCorrectAnswer { get; set; }
        // تستخدم في التصحيح فقط (لا تُرسل للطالب)

        public int Order { get; set; }
        // ترتيب الاختيار داخل السؤال (A, B, C, D)

        public int QuestionId { get; set; }

        public Question? Question { get; set; }
    }
}
