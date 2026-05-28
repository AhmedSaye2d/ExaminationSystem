using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Exam.Application.Dto.Proctoring
{
    public class ProctoringFrameRequest
    {
        [Required]
        public IFormFile Frame { get; set; } = null!;

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ExamId { get; set; }
    }
}
