using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Exam.Application.Dto.Proctoring
{
    public class ProctoringVideoRequest
    {
        [Required]
        public IFormFile Video { get; set; } = null!;

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ExamId { get; set; }

        /// <summary>
        /// Interval between analyzed frames in seconds (default: 1.0)
        /// </summary>
        public float Interval { get; set; } = 1.0f;
    }
}
