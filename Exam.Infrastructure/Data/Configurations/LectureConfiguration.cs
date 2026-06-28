using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class LectureConfiguration : IEntityTypeConfiguration<Lecture>
    {
        public void Configure(EntityTypeBuilder<Lecture> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.Description)
                .HasMaxLength(1000);

            builder.Property(l => l.VideoUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(l => l.ThumbnailUrl)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(l => l.Course)
                .WithMany(c => c.Lectures)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(l => l.Instructor)
                .WithMany(i => i.Lectures)
                .HasForeignKey(l => l.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Performance optimizations (Indexing)
            builder.HasIndex(l => l.CourseId);
            builder.HasIndex(l => l.InstructorId);
        }
    }
}
