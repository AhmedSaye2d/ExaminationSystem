using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class ExamAnswerConfiguration : IEntityTypeConfiguration<ExamAnswer>
    {
        public void Configure(EntityTypeBuilder<ExamAnswer> builder)
        {
            builder.HasKey(ea => ea.Id);

            builder.HasOne(ea => ea.ExamStudent)
                .WithMany(es => es.ExamAnswers)
                .HasForeignKey(ea => ea.ExamStudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ea => ea.Question)
                .WithMany()
                .HasForeignKey(ea => ea.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ea => ea.Choice)
                .WithMany()
                .HasForeignKey(ea => ea.ChoiceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ea => ea.Student)
                .WithMany()
                .HasForeignKey(ea => ea.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ea => ea.Exam)
                .WithMany()
                .HasForeignKey(ea => ea.ExamId)
                .OnDelete(DeleteBehavior.NoAction);

            // Performance optimizations (Indexing)
            builder.HasIndex(ea => ea.ExamStudentId);
            builder.HasIndex(ea => ea.StudentId);
            builder.HasIndex(ea => ea.ExamId);
            builder.HasIndex(ea => ea.QuestionId);
            builder.HasIndex(ea => ea.ChoiceId);

            // 🔥 SECURE: Force unique answer per question in a session
            builder.HasIndex(ea => new { ea.ExamStudentId, ea.QuestionId })
                .IsUnique();
        }
    }
}
