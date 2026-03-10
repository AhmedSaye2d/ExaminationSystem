using Exam.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Exam.Domain.Entities.Identity
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Embeddings { get; set; }
        public string? ImageURL { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public UserType UserType { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
