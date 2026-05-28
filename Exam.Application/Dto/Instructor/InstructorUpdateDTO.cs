namespace Exam.Application.Dto.Instructor
{
    public class InstructorUpdateDTO
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public DateTime? HireDate { get; set; }
    }
}
