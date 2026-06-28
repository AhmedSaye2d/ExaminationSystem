using System;
using System.Collections.Generic;

namespace Exam.Application.Dto.Lecture
{
    public class LectureDetailDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int InstructorId { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int DurationSeconds { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<LectureAttachmentDTO> Attachments { get; set; } = new();
    }
}
