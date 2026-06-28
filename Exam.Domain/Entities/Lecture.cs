using Exam.Domain.Entities.Common;
using System.Collections.Generic;

namespace Exam.Domain.Entities
{
    public class Lecture : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        public int CourseId { get; set; }
        public Course Course { get; set; } = default!;

        public int InstructorId { get; set; }
        public Instructor Instructor { get; set; } = default!;

        public string VideoUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int DurationSeconds { get; set; }
        public long FileSize { get; set; }

        public List<LectureAttachment> Attachments { get; set; } = new();
    }
}
