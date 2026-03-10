using Exam.Application.Dto.SubmitExam;
using Exam.Application.Services.Interfaces.IExamStudentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Security.Claims;

namespace Exam.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/student-exams")]
    [Authorize]
    public class StudentExamsController : ControllerBase
    {
        private readonly IStudentExamService _studentExamService;

        public StudentExamsController(IStudentExamService studentExamService)
        {
            _studentExamService = studentExamService;
        }

        /// <summary>
        /// Start a new exam session for the current authenticated student.
        /// </summary>
        /// <param name="examId">Exam ID.</param>
        /// <returns>The ID of the newly created session.</returns>
        [HttpPost("start")]
        public async Task<IActionResult> StartExam([FromQuery] int examId)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var examStudentId = await _studentExamService.StartExamAsync(examId, studentId);
            return Ok(new { message = "Exam started successfully", examStudentId });
        }

        /// <summary>
        /// Save or update an answer for a question within an active exam session.
        /// </summary>
        /// <param name="examStudentId">Exam session ID.</param>
        /// <param name="dto">Question and choice selection.</param>
        /// <returns>Success message.</returns>
        [HttpPost("{examStudentId:int}/answers")]
        public async Task<IActionResult> SaveAnswer(int examStudentId, [FromBody] StudentAnswerDTO dto)
        {
            // Note: Ideally, you'd verify here if examStudentId belongs to the current student
            await _studentExamService.SaveAnswerAsync(examStudentId, dto.QuestionId, dto.ChoiceId);
            return Ok(new { message = "Answer saved successfully" });
        }

        /// <summary>
        /// Submit an exam session for final grading.
        /// </summary>
        /// <param name="examStudentId">Exam session ID.</param>
        /// <returns>Success message.</returns>
        [HttpPost("{examStudentId:int}/submit")]
        public async Task<IActionResult> SubmitExam(int examStudentId)
        {
            await _studentExamService.SubmitExamAsync(examStudentId);
            return Ok(new { message = "Exam submitted successfully" });
        }

        /// <summary>
        /// Get the list of questions assigned to an active exam session.
        /// </summary>
        /// <param name="examStudentId">Exam session ID.</param>
        /// <returns>List of questions.</returns>
        [HttpGet("{examStudentId:int}/questions")]
        public async Task<IActionResult> GetExamQuestions(int examStudentId)
        {
            var questions = await _studentExamService.GetExamQuestionsAsync(examStudentId);
            return Ok(questions);
        }

        /// <summary>
        /// Get the grading results for a specific exam session.
        /// </summary>
        /// <param name="examStudentId">Exam session ID.</param>
        /// <returns>Result details including score.</returns>
        [HttpGet("{examStudentId:int}/result")]
        public async Task<IActionResult> GetSessionResult(int examStudentId)
        {
            var result = await _studentExamService.GetResultBySessionAsync(examStudentId);
            return Ok(result);
        }

        /// <summary>
        /// Get all results for a specific exam across all students.
        /// </summary>
        /// <param name="examId">Exam ID.</param>
        /// <returns>List of student results for the exam.</returns>
        [HttpGet("results/exam/{examId:int}")]
        public async Task<IActionResult> GetExamResults(int examId)
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var results = await _studentExamService.GetStudentResultsAsync(studentId);
            return Ok(results);
        }

        /// <summary>
        /// Get all exam results for the current authenticated student.
        /// </summary>
        /// <returns>List of student's results.</returns>
        [HttpGet("results/my-results")]
        public async Task<IActionResult> GetMyResults()
        [HttpGet("results/exam/{examId:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetExamResults(int examId)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var results = await _studentExamService.GetStudentResultsAsync(studentId);
            var results = await _studentExamService.GetExamResultsAsync(examId);
            return Ok(results);
        }

        /// <summary>
        /// Get a summary of the current student's result for a specific exam.
        /// </summary>
        /// <param name="examId">Exam ID.</param>
        /// <returns>Result summary.</returns>
        [HttpGet("results/my-summary")]
        public async Task<IActionResult> GetMyExamSummary([FromQuery] int examId)
        [HttpGet("results/summary")]
        public async Task<IActionResult> GetExamResultSummary([FromQuery] int examId)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _studentExamService.GetExamResultAsync(examId, studentId);
            return Ok(result);
        }
    }
}
