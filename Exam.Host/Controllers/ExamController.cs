using Exam.Application.Dto.Exam;
using Exam.Application.Services.Interfaces.IExamServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Exam.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/exams")]
    public class ExamController : ControllerBase
    [Authorize]
    public class ExamsController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }

        /// <summary>
        /// Retrieve all exams.
        /// </summary>
        /// <returns>A list of exams.</returns>
        [AllowAnonymous]
        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _examService.GetAllAsync();
            return Ok(res);
        }

        /// <summary>
        /// Retrieve an exam by ID.
        /// </summary>
        /// <param name="id">Exam ID.</param>
        /// <returns>The requested exam details.</returns>
        [HttpGet("my-exams")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyExams()
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _examService.GetInstructorExamsAsync(instructorId);
            return Ok(res);
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _examService.GetByIdAsync(id);
            return Ok(res);
        }

        /// <summary>
        /// Create a new exam.
        /// </summary>
        /// <param name="dto">Exam details.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("stats/{id:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetExamStats(int id)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _examService.GetExamStatsAsync(id, instructorId);
            return Ok(res);
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Create([FromBody] ExamCreateDTO dto)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            dto.InstructorId = instructorId;

            await _examService.CreateAsync(dto);
            return Ok(new { message = "Exam created successfully" });
        }

        /// <summary>
        /// Update an existing exam.
        /// </summary>
        /// <param name="id">Exam ID to update.</param>
        /// <param name="dto">Updated exam details.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("Update/{id:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ExamCreateDTO dto)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _examService.UpdateAsync(id, dto, instructorId);
            return Ok(new { message = "Exam updated successfully" });
        }

        /// <summary>
        /// Delete an exam by ID.
        /// </summary>
        /// <param name="id">Exam ID to delete.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("Delete/{id:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _examService.DeleteAsync(id, instructorId);
            return Ok(new { message = "Exam deleted successfully" });
        }
    }
}
