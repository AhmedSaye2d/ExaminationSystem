using System;

namespace Exam.Application.Dto.SubmitExam
{
    public class ExamResultDTO
    {
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public double Score { get; set; }
        public int TotalGrade { get; set; }
        public bool IsPassed { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public bool IsSubmitted { get; set; }
    }
}
