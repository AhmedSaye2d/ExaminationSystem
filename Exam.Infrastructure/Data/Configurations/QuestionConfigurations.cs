using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasKey(q => q.Id);
            builder.Property(q => q.Body).IsRequired();
            builder.Property(q => q.Header).HasMaxLength(500);
            
            builder.HasMany(q => q.Options)
                   .WithOne(o => o.Question)
                   .HasForeignKey(o => o.QuestionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
    {
        public void Configure(EntityTypeBuilder<QuestionOption> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.OptionText).IsRequired().HasMaxLength(1000);
        }
    }
}
