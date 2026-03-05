using Exam.Application.Dto.Exam;
using Exam.Application.Services.Interfaces.IExamServices;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers
{
    [ApiController]
    [Route("api/exams")]
    public class ExamsController : ControllerBase
    {
        private readonly IExamService _examService;

        public ExamsController(IExamService examService)
        {
            _examService = examService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _examService.GetAllAsync();
            return Ok(res);
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _examService.GetByIdAsync(id);
            return Ok(res);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] ExamCreateDTO dto)
        {
            await _examService.CreateAsync(dto);
            return Ok(new { message = "Exam created successfully" });
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ExamCreateDTO dto)
        {
            await _examService.UpdateAsync(id, dto);
            return Ok(new { message = "Exam updated successfully" });
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _examService.DeleteAsync(id);
            return Ok(new { message = "Exam deleted successfully" });
        }

        [HttpPost("{id:int}/questions")]
        public async Task<IActionResult> AddQuestionsToExam(int id, [FromBody] IEnumerable<int> questionIds)
        {
            await _examService.AddQuestionsToExamAsync(id, questionIds);
            return Ok(new { message = "Questions added to exam successfully" });
        }

        [HttpDelete("{examId:int}/questions/{questionId:int}")]
        public async Task<IActionResult> RemoveQuestionFromExam(int examId, int questionId)
        {
            await _examService.RemoveQuestionFromExamAsync(examId, questionId);
            return Ok(new { message = "Question removed from exam successfully" });
        }
    }
}
