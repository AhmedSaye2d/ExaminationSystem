using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exam.Infrastructure.Data.Configurations
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasKey(q => q.Id);
            // المفتاح الأساسي للسؤال

            builder.Property(q => q.Text)
                .IsRequired()
                .HasMaxLength(1000);
            // نص السؤال إجباري

            builder.Property(q => q.Type)
                .IsRequired();
            // نوع السؤال (MCQ - TF - Essay)

            builder.HasMany(q => q.Choices)
                .WithOne(c => c.Question)
                .HasForeignKey(c => c.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            // كل سؤال له عدة اختيارات

            builder.HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .IsRequired(false) // This is now optional
                .OnDelete(DeleteBehavior.SetNull); // Don't delete question if exam is deleted
            // كل سؤال ينتمي لامتحان واحد

            // Performance optimizations (Indexing)
            builder.HasIndex(q => q.ExamId);
            builder.HasIndex(q => q.Type);
        }
    }
}
