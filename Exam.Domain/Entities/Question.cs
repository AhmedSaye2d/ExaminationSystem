using Exam.Domain.Common;
using Exam.Domain.Entities.Common;
using Exam.Domain.Enum;

namespace Exam.Domain.Entities
{
    public class Question : BaseEntity
    // يمثل السؤال في بنك الأسئلة القابل لإعادة الاستخدام

    {
        public string Text { get; set; } = string.Empty;
        // نص السؤال المعروض للطالب

        public QuestionType Type { get; set; }
        // نوع السؤال (اختياري - صح/خطأ - مقالي)
        // يحدد أسلوب التصحيح (تلقائي أو يدوي)

        public int DifficultyLevel { get; set; }
        // مستوى صعوبة السؤال (1 سهل - 2 متوسط - 3 صعب)
        // مفيد في توليد امتحانات عشوائية مستقبلاً

        public HashSet<Choice> Choices { get; set; } = new();
        // اختيارات السؤال (تستخدم فقط في MCQ و True/False)

        public int ExamId { get; set; }
        public Exam Exam { get; set; }
        // السؤال ينتمي لامتحان واحد محدد

        public int Grade { get; set; }
        // درجة السؤال (تستخدم في حساب النتيجة)
    }
}
