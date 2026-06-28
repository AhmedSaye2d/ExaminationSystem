using Exam.Application.Dto.Lecture;
using Exam.Application.Services.Interfaces;
using Exam.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Exam.Host.Controllers
{
    [Authorize]
    [ApiController]
    public class LectureController : ControllerBase
    {
        private readonly ILectureService _lectureService;

        public LectureController(ILectureService lectureService)
        {
            _lectureService = lectureService;
        }

        /// <summary>
        /// Upload a new lecture for a course (Instructor/Admin only)
        /// </summary>
        [HttpPost("api/instructor/lectures")]
        [Authorize(Roles = AppRoles.Instructor + "," + AppRoles.Admin)]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(2_147_483_648)] // 2GB max
        [RequestFormLimits(MultipartBodyLengthLimit = 2_147_483_648)]
        public async Task<IActionResult> UploadLecture([FromForm] UploadLectureDTO dto, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _lectureService.UploadLectureAsync(dto, userId, cancellationToken);
            return CreatedAtAction(nameof(GetLectureDetails), new { lectureId = result.Id }, result);
        }

        /// <summary>
        /// Get all lectures for a specific course (Students must be enrolled; Instructors/Admins must be assigned)
        /// </summary>
        [HttpGet("api/courses/{courseId:int}/lectures")]
        public async Task<IActionResult> GetCourseLectures(int courseId, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _lectureService.GetCourseLecturesAsync(courseId, userId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get specific lecture details (Students must be enrolled; Instructors/Admins must be assigned)
        /// </summary>
        [HttpGet("api/lectures/{lectureId:int}")]
        public async Task<IActionResult> GetLectureDetails(int lectureId, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _lectureService.GetLectureDetailsAsync(lectureId, userId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Update lecture details (Instructor/Admin only)
        /// </summary>
        [HttpPut("api/instructor/lectures/{id:int}")]
        [Authorize(Roles = AppRoles.Instructor + "," + AppRoles.Admin)]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(2_147_483_648)] // 2GB max
        [RequestFormLimits(MultipartBodyLengthLimit = 2_147_483_648)]
        public async Task<IActionResult> UpdateLecture(int id, [FromForm] UpdateLectureDTO dto, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _lectureService.UpdateLectureAsync(id, dto, userId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Delete a lecture (Instructor/Admin only)
        /// </summary>
        [HttpDelete("api/instructor/lectures/{id:int}")]
        [Authorize(Roles = AppRoles.Instructor + "," + AppRoles.Admin)]
        public async Task<IActionResult> DeleteLecture(int id, CancellationToken cancellationToken)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _lectureService.DeleteLectureAsync(id, userId, cancellationToken);
            return Ok(new { message = "Lecture deleted successfully" });
        }
    }
}
