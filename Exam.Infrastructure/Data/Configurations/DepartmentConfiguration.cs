using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(d => d.Id);
            // Primary Key

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(150);
            // اسم القسم إجباري

            // Performance optimizations (Indexing)
            builder.HasIndex(d => d.Name);
        }
    }
}