using Exam.Domain.Entities.Common;
using System;

namespace Exam.Domain.Entities.Identity
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;
    }
}
