using System.Collections.Generic;
using Exam.Application.Dto.Question;

namespace Exam.Application.Dto.SubmitExam
{
    public class ResumeExamDTO
    {
        public int ExamStudentId { get; set; }
        public int ExamId { get; set; }
        public IEnumerable<QuestionForStudentDTO> Questions { get; set; } = new List<QuestionForStudentDTO>();
        public IEnumerable<ExamAnswerDTO> SavedAnswers { get; set; } = new List<ExamAnswerDTO>();
        public int RemainingMinutes { get; set; }
    }
}
