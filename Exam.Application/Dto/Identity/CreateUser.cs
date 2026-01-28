using Exam.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Identity
{
    public class CreateUser:BaseModel
    {
        public required string FullName { get; set; }
        public required string ConfirmPassword { get; set; }
        public UserType UserType { get; set; } = UserType.Student;
    }
}
