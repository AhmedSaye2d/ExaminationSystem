using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Common
{
    public record ServiceResponse(bool Success, string Message)
    {
        public static ServiceResponse Ok(string message = "Success")
            => new(true, message);

        public static ServiceResponse Fail(string message)
            => new(false, message);
    }

}
