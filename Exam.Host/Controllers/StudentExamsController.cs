using Exam.Application.Dto.SubmitExam;
using Exam.Application.Services.Interfaces.IExamStudentServices;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers
{
    [ApiController]
    [Route("api/student-exams")]
    public class StudentExamsController : ControllerBase
    {
        private readonly IStudentExamService _studentExamService;

        public StudentExamsController(IStudentExamService studentExamService)
        {
            _studentExamService = studentExamService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartExam([FromQuery] int examId, [FromQuery] int studentId)
        {
            var examStudentId = await _studentExamService.StartExamAsync(examId, studentId);
            return Ok(new { message = "Exam started successfully", examStudentId });
        }

        [HttpPost("{examStudentId:int}/answers")]
        public async Task<IActionResult> SaveAnswer(int examStudentId, [FromBody] StudentAnswerDTO dto)
        {
            await _studentExamService.SaveAnswerAsync(examStudentId, dto.QuestionId, dto.ChoiceId);
            return Ok(new { message = "Answer saved successfully" });
        }

        [HttpPost("{examStudentId:int}/submit")]
        public async Task<IActionResult> SubmitExam(int examStudentId)
        {
            await _studentExamService.SubmitExamAsync(examStudentId);
            return Ok(new { message = "Exam submitted successfully" });
        }

        [HttpGet("{examStudentId:int}/questions")]
        public async Task<IActionResult> GetExamQuestions(int examStudentId)
        {
            var questions = await _studentExamService.GetExamQuestionsAsync(examStudentId);
            return Ok(questions);
        }

        [HttpGet("{examStudentId:int}/result")]
        public async Task<IActionResult> GetSessionResult(int examStudentId)
        {
            var result = await _studentExamService.GetResultBySessionAsync(examStudentId);
            return Ok(result);
        }

        [HttpGet("results/exam/{examId:int}")]
        public async Task<IActionResult> GetExamResults(int examId)
        {
            var results = await _studentExamService.GetExamResultsAsync(examId);
            return Ok(results);
        }

        [HttpGet("results/student/{studentId:int}")]
        public async Task<IActionResult> GetStudentResults(int studentId)
        {
            var results = await _studentExamService.GetStudentResultsAsync(studentId);
            return Ok(results);
        }

        [HttpGet("results/summary")]
        public async Task<IActionResult> GetExamResult([FromQuery] int examId, [FromQuery] int studentId)
        {
            var result = await _studentExamService.GetExamResultAsync(examId, studentId);
            return Ok(result);
        }
    }
}
