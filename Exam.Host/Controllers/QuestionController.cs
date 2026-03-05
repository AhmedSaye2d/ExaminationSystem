using Exam.Application.Dto.Question;
using Exam.Application.Services.Interfaces.IQuestionServices;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers
{
    [ApiController]
    [Route("api/questions")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? courseId = null)
        {
            var res = await _questionService.GetPagedAsync(page, pageSize, courseId);
            return Ok(new { data = res.Items, totalCount = res.TotalCount, page, pageSize });
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _questionService.GetAllAsync();
            return Ok(res);
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _questionService.GetByIdAsync(id);
            return Ok(res);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] QuestionCreateDTO dto)
        {
            await _questionService.CreateAsync(dto);
            return Ok(new { message = "Question created successfully" });
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuestionCreateDTO dto)
        {
            await _questionService.UpdateAsync(id, dto);
            return Ok(new { message = "Question updated successfully" });
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _questionService.DeleteAsync(id);
            return Ok(new { message = "Question deleted successfully" });
        }

        [HttpPost("with-choices")]
        public async Task<IActionResult> AddQuestionWithChoices([FromBody] QuestionWithChoicesDTO dto)
        {
            var id = await _questionService.AddQuestionWithChoicesAsync(dto);
            return Ok(new { Id = id });
        }
    }
}
