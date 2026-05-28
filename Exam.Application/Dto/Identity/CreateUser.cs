using Exam.Domain.Enum;

namespace Exam.Application.Dto.Identity
{
    public class CreateUser : BaseModel
    {
        public required string FullName { get; set; }
        public required string ConfirmPassword { get; set; }
        public UserType UserType { get; set; } = UserType.Student;
    }
}
