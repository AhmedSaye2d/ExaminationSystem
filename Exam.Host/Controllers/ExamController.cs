using Exam.Application.Dto.Exam;
using Exam.Application.Services.Interfaces.IExamServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Exam.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/exams")]
    public class ExamController : ControllerBase
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
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] ExamCreateDTO dto)
        {
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
        public async Task<IActionResult> Update(int id, [FromBody] ExamCreateDTO dto)
        {
            await _examService.UpdateAsync(id, dto);
            return Ok(new { message = "Exam updated successfully" });
        }

        /// <summary>
        /// Delete an exam by ID.
        /// </summary>
        /// <param name="id">Exam ID to delete.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _examService.DeleteAsync(id);
            return Ok(new { message = "Exam deleted successfully" });
        }

        /// <summary>
        /// Add multiple questions to an exam.
        /// </summary>
        /// <param name="id">Exam ID.</param>
        /// <param name="questionIds">List of question IDs to add.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("{id:int}/questions")]
        public async Task<IActionResult> AddQuestionsToExam(int id, [FromBody] IEnumerable<int> questionIds)
        {
            await _examService.AddQuestionsToExamAsync(id, questionIds);
            return Ok(new { message = "Questions added to exam successfully" });
        }

        /// <summary>
        /// Remove a question from a specific exam.
        /// </summary>
        /// <param name="examId">Exam ID.</param>
        /// <param name="questionId">Question ID to remove.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{examId:int}/questions/{questionId:int}")]
        public async Task<IActionResult> RemoveQuestionFromExam(int examId, int questionId)
        {
            await _examService.RemoveQuestionFromExamAsync(examId, questionId);
            return Ok(new { message = "Question removed from exam successfully" });
        }
    }
}
