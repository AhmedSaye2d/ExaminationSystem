namespace Exam.Application.Dto.Admin
{
    public class AdminDashboardDTO
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public int TotalExams { get; set; }
        public int PendingExams { get; set; }
        public int ActiveExams { get; set; }
    }
}
