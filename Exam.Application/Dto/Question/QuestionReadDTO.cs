using Exam.Application.Dto.Choice;
using System;

namespace Exam.Application.Dto.Question
{
    public class QuestionReadDTO
    {
        public int Id { get; set; }
        // معرف السؤال

        public string Text { get; set; } = string.Empty;
        // نص السؤال

        public int Grade { get; set; }
        // درجة السؤال

        public List<ChoiceReadDTO> Choices { get; set; } = new();
        // عرض الاختيارات مع معرفة الإجابة الصحيحة
    }
}
