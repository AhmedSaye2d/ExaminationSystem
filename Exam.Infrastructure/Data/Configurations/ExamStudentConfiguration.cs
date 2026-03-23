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
    public class ExamStudentConfiguration : IEntityTypeConfiguration<ExamStudent>
    {
        public void Configure(EntityTypeBuilder<ExamStudent> builder)
        {
            builder.HasKey(es => es.Id);
            // Primary Key لمحاولة الامتحان

            builder.HasOne(es => es.Student)
                .WithMany(s => s.ExamStudents)
                .HasForeignKey(es => es.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            // الطالب يمكنه دخول عدة امتحانات

            builder.HasOne(es => es.Exam)
                .WithMany(e => e.ExamStudents)
                .HasForeignKey(es => es.ExamId)
                .OnDelete(DeleteBehavior.Restrict);
            // الامتحان يحتوي على عدة طلاب

            builder.HasMany(es => es.ExamAnswers)
                .WithOne(a => a.ExamStudent)
                .HasForeignKey(a => a.ExamStudentId);
            // جميع إجابات الطالب داخل الامتحان

            builder.HasIndex(es => new { es.StudentId, es.ExamId })
                .IsUnique();
            // يمنع الطالب من دخول نفس الامتحان أكثر من مرة (محاولة واحدة)

            // Performance optimizations (Indexing)
            builder.HasIndex(es => es.ExamId);
            builder.HasIndex(es => es.Status);
        }
    }
}
