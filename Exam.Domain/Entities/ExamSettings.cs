namespace Exam.Domain.Entities
{
    /// <summary>
    /// Stored as Owned Entity inside Exams table (no separate table).
    /// </summary>
    public class ExamSettings
    {
        public bool ShuffleQuestions { get; set; } = false;
        // هل يتم خلط ترتيب الأسئلة عشوائياً لكل طالب

        public bool ShuffleChoices { get; set; } = false;
        // هل يتم خلط الاختيارات داخل كل سؤال

        public int DurationMinutes { get; set; } = 60;
        // مدة الامتحان بالدقائق

        public bool ShowResultAfterSubmit { get; set; } = true;
        // هل تظهر نتيجة الطالب مباشرة بعد تسليم الامتحان

        public bool AllowReview { get; set; } = false;
        // هل يُسمح للطالب بمراجعة إجاباته بعد التسليم

        public int MaxAttempts { get; set; } = 1;
        // أقصى عدد محاولات مسموح بها لكل طالب
    }
}
