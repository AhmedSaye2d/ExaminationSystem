using Exam.Application.Dto.SubmitExam;
using Exam.Application.Services.Interfaces.IExamStudentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exam.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/student-exams")]
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
        [HttpPost("start/{examId:int}")]
        public async Task<IActionResult> StartExam(int examId)
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
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _studentExamService.SaveAnswerAsync(examStudentId, studentId, dto.QuestionId, dto.ChoiceId);
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
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _studentExamService.SubmitExamAsync(examStudentId, studentId);
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
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var questions = await _studentExamService.GetExamQuestionsAsync(examStudentId, studentId);
            return Ok(questions);
        }

        /// <summary>
        /// Resume an exam session (return saved answers, remaining time and questions)
        /// </summary>
        [HttpGet("{examStudentId:int}/resume")]
        public async Task<IActionResult> ResumeExam(int examStudentId)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var resume = await _studentExamService.ResumeExamAsync(examStudentId, studentId);
            return Ok(resume);
        }

        /// <summary>
        /// Get the grading results for a specific exam session.
        /// </summary>
        /// <param name="examStudentId">Exam session ID.</param>
        /// <returns>Result details including score.</returns>
        [HttpGet("{examStudentId:int}/result")]
        public async Task<IActionResult> GetSessionResult(int examStudentId)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _studentExamService.GetResultBySessionAsync(examStudentId, studentId);
            return Ok(result);
        }



        /// <summary>
        /// Get all exam results for the current authenticated student.
        /// </summary>
        /// <returns>List of student's results.</returns>
        [HttpGet("results/my-results")]
        public async Task<IActionResult> GetMyResults([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _studentExamService.GetStudentResultsPagedAsync(studentId, page, pageSize);
            return Ok(new { data = res.Items, totalCount = res.TotalCount, page, pageSize });
        }

        /// <summary>
        /// [Admin/Instructor Only] Get paged results for a specific exam.
        /// </summary>
        [HttpGet("results/exam/{examId:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetExamResults(int examId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var res = await _studentExamService.GetExamResultsPagedAsync(examId, page, pageSize);
            return Ok(new { data = res.Items, totalCount = res.TotalCount, page, pageSize });
        }

        /// <summary>
        /// Get a summary of the current student's result for a specific exam.
        /// </summary>
        /// <param name="examId">Exam ID.</param>
        /// <returns>Result summary.</returns>
        [HttpGet("results/my-summary")]
        public async Task<IActionResult> GetMyExamSummary([FromQuery] int examId)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _studentExamService.GetExamResultAsync(examId, studentId);
            return Ok(result);
        }
    }
}
