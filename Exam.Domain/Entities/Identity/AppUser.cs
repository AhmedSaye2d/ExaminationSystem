using Exam.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Exam.Domain.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        
        // Relationships
        public ICollection<Exam> CreatedExams { get; set; } = [];
        public ICollection<ExamAttempt> ExamAttempts { get; set; } = [];
    }
}
