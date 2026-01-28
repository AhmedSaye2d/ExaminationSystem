using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Identity
{
    public class BaseModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
