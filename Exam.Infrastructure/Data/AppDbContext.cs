using Exam.Domain.Entities;
using Exam.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Exam.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
             : base(options)
        {
        }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Domain.Entities.Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<ExamAttempt> ExamAttempts { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(builder);
        }
    }
}
