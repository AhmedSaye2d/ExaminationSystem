using Exam.Application.Dto.Choice;
namespace Exam.Application.Dto.Question
{
    public class QuestionCreateDTO
    {
        public int ExamId { get; set; }
        public string Text { get; set; } = string.Empty;
        // نص السؤال

        public int Grade { get; set; }
        // درجة السؤال (تُستخدم في حساب النتيجة)

        public int Type { get; set; }
        // نوع السؤال (MCQ - True/False - ...)

        public List<ChoiceCreateDTO> Choices { get; set; } = new();
        // قائمة الاختيارات المرتبطة بالسؤال
        // تُضاف مع السؤال في نفس العملية
    }
}
