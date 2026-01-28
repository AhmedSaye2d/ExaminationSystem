using Exam.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Domain.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public UserType UserType { get; set; }   // 👈 هنا

        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }

}
