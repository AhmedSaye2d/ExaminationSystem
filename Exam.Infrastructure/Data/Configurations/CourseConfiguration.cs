using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);
        // المفتاح الأساسي للجدول

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);
        // اسم الكورس إجباري وبطول محدد

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);
        // كود الكورس مثل (CS101)

        builder.HasOne(c => c.Department)
            .WithMany(d => d.Courses)
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        // علاقة القسم مع الكورسات (One-to-Many)
    }
}
