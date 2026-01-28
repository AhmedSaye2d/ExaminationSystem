using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Application.Dto.Identity
{
    public record LoginResponse(
       bool Success,
       string Message,
       string? Token = null,
       string? RefreshToken = null
   );

}
