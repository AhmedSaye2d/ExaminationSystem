using Microsoft.AspNetCore.Identity;

namespace Exam.Domain.Entities.Identity
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; }
    }
}
