namespace Exam.Application.Dto.Instructor
{
    public class InstructorCreateDTO
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public DateTime? HireDate { get; set; }
    }
}