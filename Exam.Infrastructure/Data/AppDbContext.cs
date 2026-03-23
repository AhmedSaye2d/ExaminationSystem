using Exam.Domain.Entities;
using Exam.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace Exam.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        public DbSet<Choice> Choices { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseInstructor> CourseInstructors { get; set; }
        public DbSet<CourseStudent> CourseStudents { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Exam.Domain.Entities.Exam> Exams { get; set; }
        public DbSet<ExamAnswer> ExamAnswers { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<ExamStudent> ExamStudents { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // ===================== Global Soft-Delete Filters =====================
            builder.Entity<AppUser>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Exam.Domain.Entities.Exam>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Question>().HasQueryFilter(q => !q.IsDeleted);
            builder.Entity<Choice>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
            builder.Entity<ExamStudent>().HasQueryFilter(es => !es.IsDeleted);
            builder.Entity<ExamAnswer>().HasQueryFilter(ea => !ea.IsDeleted);
            builder.Entity<ExamResult>().HasQueryFilter(er => !er.IsDeleted);
            builder.Entity<CourseStudent>().HasQueryFilter(cs => !cs.IsDeleted);
            builder.Entity<CourseInstructor>().HasQueryFilter(ci => !ci.IsDeleted);
            builder.Entity<RefreshToken>().HasQueryFilter(rt => !rt.IsDeleted);
        }
    }
}