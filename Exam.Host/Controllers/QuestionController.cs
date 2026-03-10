using Exam.Application.Dto.Question;
using Exam.Application.Services.Interfaces.IQuestionServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Exam.Host.Controllers
{
    [Authorize(Roles = "Instructor,Admin")]
    [ApiController]
    [Route("api/questions")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Retrieve a paged list of questions, optionally filtered by course.
        /// </summary>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="pageSize">Number of items per page (default 10).</param>
        /// <param name="courseId">Optional course filter ID.</param>
        /// <returns>A paged list of questions and total count.</returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? courseId = null)
        {
            var res = await _questionService.GetPagedAsync(page, pageSize, courseId);
            return Ok(new { data = res.Items, totalCount = res.TotalCount, page, pageSize });
        }

        /// <summary>
        /// Retrieve all questions in the bank.
        /// </summary>
        /// <returns>A list of all questions.</returns>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _questionService.GetAllAsync();
            return Ok(res);
        }

        /// <summary>
        /// Get details of a specific question by ID.
        /// </summary>
        /// <param name="id">Question ID.</param>
        /// <returns>The requested question details.</returns>
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _questionService.GetByIdAsync(id);
            return Ok(res);
        }

        /// <summary>
        /// Create a new question.
        /// </summary>
        /// <param name="dto">Question details.</param>
        /// <returns>Success message.</returns>
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] QuestionCreateDTO dto)
        {
            await _questionService.CreateAsync(dto);
            return Ok(new { message = "Question created successfully" });
        }

        /// <summary>
        /// Update an existing question.
        /// </summary>
        /// <param name="id">Question ID to update.</param>
        /// <param name="dto">Updated question data.</param>
        /// <returns>Success message.</returns>
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuestionCreateDTO dto)
        {
            await _questionService.UpdateAsync(id, dto);
            return Ok(new { message = "Question updated successfully" });
        }

        /// <summary>
        /// Delete a question by ID.
        /// </summary>
        /// <param name="id">Question ID to delete.</param>
        /// <returns>Success message.</returns>
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _questionService.DeleteAsync(id);
            return Ok(new { message = "Question deleted successfully" });
        }

        /// <summary>
        /// Create a question along with its multiple choices in a single request.
        /// </summary>
        /// <param name="dto">Question and choices data.</param>
        /// <returns>ID of the newly created question.</returns>
        [HttpPost("with-choices")]
        public async Task<IActionResult> AddQuestionWithChoices([FromBody] QuestionWithChoicesDTO dto)
        {
            var id = await _questionService.AddQuestionWithChoicesAsync(dto);
            return Ok(new { Id = id });
        }
    }
}
