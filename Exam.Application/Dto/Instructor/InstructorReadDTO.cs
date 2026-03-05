namespace Exam.Application.Dto.Instructor
{
    public class InstructorReadDTO
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public DateTime? HireDate { get; set; }
    }
}
