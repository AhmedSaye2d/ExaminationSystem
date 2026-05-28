using Exam.Domain.Enum;

namespace Exam.Application.Dto.Student
{
    public class StudentCreateDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Gender Gender { get; set; }

        public int MajorId { get; set; }
    }
}
