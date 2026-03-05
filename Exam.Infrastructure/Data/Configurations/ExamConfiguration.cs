using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class ExamConfiguration : IEntityTypeConfiguration<Exam.Domain.Entities.Exam>
    {
        public void Configure(EntityTypeBuilder<Exam.Domain.Entities.Exam> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.Description)
                   .HasMaxLength(1000);

            builder.HasOne(e => e.Course)
                   .WithMany(c => c.Exams)
                   .HasForeignKey(e => e.CourseID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Instructor)
                   .WithMany(i => i.Exams)
                   .HasForeignKey(e => e.InstructorID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsOne(e => e.Settings);
        }
    }
}
