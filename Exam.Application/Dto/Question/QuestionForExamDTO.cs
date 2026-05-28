using Exam.Application.Dto.Choice;

namespace Exam.Application.Dto.Question
{
    public class QuestionForExamDTO
    {
        public int Id { get; set; }
        // معرف السؤال داخل الامتحان

        public string Text { get; set; } = string.Empty;
        // نص السؤال الظاهر للطالب

        public int Grade { get; set; }
        // درجة السؤال (قد تُخفى حسب الإعدادات)

        public List<ChoiceForStudentDTO> Choices { get; set; } = new();
        // اختيارات بدون الإجابة الصحيحة
    }
}
