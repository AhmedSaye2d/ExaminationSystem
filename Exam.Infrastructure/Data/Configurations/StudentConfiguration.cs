using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {

            builder.HasOne(s => s.Major)
                .WithMany(d => d.Students)
                .HasForeignKey(s => s.MajorId)
                .OnDelete(DeleteBehavior.Restrict);
            // ربط الطالب بالقسم الأكاديمي
        }
    }
}
