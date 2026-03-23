using System;

namespace Exam.Application.Dto.Exam
{
    public class InstructorExamResultDTO
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public double Score { get; set; }
        public double Percentage { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
