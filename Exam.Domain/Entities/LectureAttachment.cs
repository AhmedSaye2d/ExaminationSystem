using Exam.Domain.Entities.Common;

namespace Exam.Domain.Entities
{
    public class LectureAttachment : BaseEntity
    {
        public int LectureId { get; set; }
        public Lecture Lecture { get; set; } = default!;

        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
