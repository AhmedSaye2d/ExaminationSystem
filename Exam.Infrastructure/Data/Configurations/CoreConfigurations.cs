using Exam.Domain.Entities;
using Exam.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
            builder.Property(s => s.Description).HasMaxLength(1000);

            builder.HasMany(s => s.Exams)
                   .WithOne(e => e.Subject)
                   .HasForeignKey(e => e.SubjectId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ExamConfiguration : IEntityTypeConfiguration<Domain.Entities.Exam>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Exam> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
            builder.Property(e => e.Description).HasMaxLength(2000);
            builder.Property(e => e.Instructions).HasMaxLength(4000);

            builder.HasMany(e => e.Questions)
                   .WithOne(q => q.Exam)
                   .HasForeignKey(q => q.ExamId)
                   .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(e => e.Instructor)
                   .WithMany(u => u.CreatedExams)
                   .HasForeignKey(e => e.InstructorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.InstructorId);
        }
    }
}
