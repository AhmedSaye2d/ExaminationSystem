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
    public class ChoiceConfiguration : IEntityTypeConfiguration<Choice>
    {
        public void Configure(EntityTypeBuilder<Choice> builder)
        {
            // Table Name
            builder.ToTable("Choices");

            // Primary Key
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.Text)
                   .IsRequired()
                   .HasMaxLength(500);
            // نص الاختيار (إجباري وبحد أقصى 500 حرف)

            builder.Property(c => c.IsCorrectAnswer)
                   .IsRequired();
            // هل هذا الاختيار هو الإجابة الصحيحة أم لا

            // Relationship: Choice -> Question (Many to One)
            builder.HasOne(c => c.Question)
                   .WithMany(q => q.Choices)
                   .HasForeignKey(c => c.QuestionId)
                   .OnDelete(DeleteBehavior.Cascade);
            // كل Question ليه اختيارات كتير
            // وكل Choice تابع لسؤال واحد
            // ولو السؤال اتحذف تتحذف الاختيارات تلقائياً
        }
    }
}

