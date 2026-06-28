using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Exam.Application.Dto.Lecture
{
    public class UpdateLectureDTO
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public IFormFile? VideoFile { get; set; }

        public List<IFormFile>? AttachmentFiles { get; set; }
    }
}
