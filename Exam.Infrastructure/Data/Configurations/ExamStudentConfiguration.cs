using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class ExamStudentConfiguration : IEntityTypeConfiguration<ExamStudent>
    {
        public void Configure(EntityTypeBuilder<ExamStudent> builder)
        {
            builder.HasKey(es => es.Id);
            // Primary Key لمحاولة الامتحان

            builder.HasOne(es => es.Student)
                .WithMany(s => s.ExamStudents)
                .HasForeignKey(es => es.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            // الطالب يمكنه دخول عدة امتحانات

            builder.HasOne(es => es.Exam)
                .WithMany(e => e.ExamStudents)
                .HasForeignKey(es => es.ExamId)
                .OnDelete(DeleteBehavior.Restrict);
            // الامتحان يحتوي على عدة طلاب

            builder.HasMany(es => es.ExamAnswers)
                .WithOne(a => a.ExamStudent)
                .HasForeignKey(a => a.ExamStudentId);
            // جميع إجابات الطالب داخل الامتحان

            // Allow a student to have multiple entries for the same exam (multiple attempts)

            // Performance optimizations (Indexing)
            builder.HasIndex(es => es.ExamId);
            builder.HasIndex(es => es.Status);
        }
    }
}
