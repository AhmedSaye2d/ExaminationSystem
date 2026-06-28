using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Exam.Application.Dto.Lecture
{
    public class UploadLectureDTO
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public IFormFile VideoFile { get; set; } = default!;

        public List<IFormFile>? AttachmentFiles { get; set; }
    }
}
