using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class ExamResultConfiguration : IEntityTypeConfiguration<ExamResult>
    {
        public void Configure(EntityTypeBuilder<ExamResult> builder)
        {
            builder.HasKey(er => er.Id);

            builder.HasOne(er => er.ExamStudent)
                .WithMany()
                .HasForeignKey(er => er.ExamStudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(er => er.Student)
                .WithMany()
                .HasForeignKey(er => er.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(er => er.Exam)
                .WithMany()
                .HasForeignKey(er => er.ExamId)
                .OnDelete(DeleteBehavior.NoAction);

            // Performance optimizations (Indexing)
            builder.HasIndex(er => er.ExamStudentId);
            builder.HasIndex(er => er.StudentId);
            builder.HasIndex(er => er.ExamId);
        }
    }
}
