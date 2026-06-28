using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Infrastructure.Data.Configurations
{
    public class LectureAttachmentConfiguration : IEntityTypeConfiguration<LectureAttachment>
    {
        public void Configure(EntityTypeBuilder<LectureAttachment> builder)
        {
            builder.HasKey(la => la.Id);

            builder.Property(la => la.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(la => la.FileUrl)
                .IsRequired()
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(la => la.Lecture)
                .WithMany(l => l.Attachments)
                .HasForeignKey(la => la.LectureId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index
            builder.HasIndex(la => la.LectureId);
        }
    }
}
