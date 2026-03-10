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
        }
    }
}
