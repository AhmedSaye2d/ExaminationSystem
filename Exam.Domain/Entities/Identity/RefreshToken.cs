using Microsoft.AspNetCore.Identity;

namespace Exam.Domain.Entities.Identity
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        public bool IsRevoked { get; set; } = false;

        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;
    }
}
