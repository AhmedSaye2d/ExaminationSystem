using Exam.Domain.Entities;
using Exam.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class ExamAttemptConfiguration : IEntityTypeConfiguration<ExamAttempt>
    {
        public void Configure(EntityTypeBuilder<ExamAttempt> builder)
        {
            builder.HasKey(ea => ea.Id);
            
            builder.HasOne(ea => ea.Student)
                   .WithMany(u => u.ExamAttempts)
                   .HasForeignKey(ea => ea.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ea => ea.StudentId);

            builder.HasMany(ea => ea.StudentAnswers)
                   .WithOne(sa => sa.ExamAttempt)
                   .HasForeignKey(sa => sa.ExamAttemptId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ea => ea.Exam)
                   .WithMany()
                   .HasForeignKey(ea => ea.ExamId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class StudentAnswerConfiguration : IEntityTypeConfiguration<StudentAnswer>
    {
        public void Configure(EntityTypeBuilder<StudentAnswer> builder)
        {
            builder.HasKey(sa => sa.Id);

            builder.HasOne(sa => sa.Question)
                   .WithMany()
                   .HasForeignKey(sa => sa.QuestionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sa => sa.SelectedOption)
                   .WithMany()
                   .HasForeignKey(sa => sa.SelectedOptionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
