using Exam.Application.Dto.Exam;
using Exam.Application.Services.Interfaces.IExamServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _examService.GetAllAsync();
            return Ok(res);
        }

        /// <summary>
        /// Retrieve a paged list of exams, optionally filtered by course.
        /// </summary>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <param name="courseId">Optional course ID to filter exams.</param>
        /// <returns>A paged list of exams with metadata.</returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? courseId = null)
        {
            var res = await _examService.GetPagedAsync(page, pageSize, courseId);
            return Ok(new { data = res.Items, totalCount = res.TotalCount, page, pageSize });
        }

        /// <summary>
        /// Retrieve exams created by the currently authenticated instructor.
        /// </summary>
        /// <returns>A list of the instructor's exams.</returns>
        [HttpGet("my-exams")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetMyExams()
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _examService.GetInstructorExamsAsync(instructorId);
            return Ok(res);
        }

        /// <summary>
        /// Retrieve an exam by its ID.
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
        /// Retrieve statistics for a specific exam.
        /// </summary>
        /// <param name="id">Exam ID.</param>
        /// <returns>Exam statistics.</returns>
        [HttpGet("stats/{id:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetExamStats(int id)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _examService.GetExamStatsAsync(id, instructorId);
            return Ok(res);
        }

        /// <summary>
        /// Create a new exam.
        /// </summary>
        /// <param name="dto">Exam details.</param>
        /// <returns>Success message.</returns>
        [HttpPost("Create")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Create([FromBody] ExamCreateDTO dto)
        {
            // Instructors can only create exams for themselves.
            // Admins may specify any instructorId in the request body.
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                var tokenUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                dto.InstructorId = tokenUserId;
            }

            await _examService.CreateAsync(dto);
            return Ok(new { message = "Exam created successfully" });
        }

        /// <summary>
        /// Update an existing exam.
        /// </summary>
        /// <param name="id">Exam ID to update.</param>
        /// <param name="dto">Updated exam details.</param>
        /// <returns>Success message.</returns>
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
        [HttpDelete("Delete/{id:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _examService.DeleteAsync(id, instructorId);
            return Ok(new { message = "Exam deleted successfully" });
        }

        /// <summary>
        /// Schedule an exam by setting its start time, end time, and duration.
        /// </summary>
        /// <param name="id">The ID of the exam to schedule.</param>
        /// <param name="dto">The scheduling details (start, end, duration).</param>
        /// <returns>A success message.</returns>
        [HttpPut("{id:int}/schedule")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Schedule(int id, [FromBody] ScheduleExamDTO dto)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _examService.ScheduleExamAsync(id, dto, instructorId);
            return Ok(new { message = "Exam scheduled successfully" });
        }

        /// <summary>
        /// Publish an exam to make it visible to students.
        /// </summary>
        /// <param name="id">The ID of the exam to publish.</param>
        /// <returns>A success message.</returns>
        [HttpPost("{id:int}/publish")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Publish(int id)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _examService.PublishExamAsync(id, instructorId);
            return Ok(new { message = "Exam published" });
        }

        /// <summary>
        /// Unpublish an exam to hide it from students.
        /// </summary>
        /// <param name="id">The ID of the exam to unpublish.</param>
        /// <returns>A success message.</returns>
        [HttpPost("{id:int}/unpublish")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Unpublish(int id)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _examService.UnpublishExamAsync(id, instructorId);
            return Ok(new { message = "Exam unpublished" });
        }

        /// <summary>
        /// Retrieve all questions for a specific exam.
        /// </summary>
        /// <param name="examId">Exam ID.</param>
        /// <returns>A list of questions belonging to the exam.</returns>
        [HttpGet("{examId:int}/questions")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetQuestions(int examId)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _examService.GetQuestionsByExamIdAsync(examId, instructorId);
            return Ok(res);
        }

        /// <summary>
        /// Add an existing question to a specific exam.
        /// </summary>
        /// <param name="examId">The ID of the exam.</param>
        /// <param name="dto">The question details (ID, points, display order).</param>
        /// <returns>A success message.</returns>
        [HttpPost("{examId:int}/questions")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> AddQuestion(int examId, [FromBody] Exam.Application.Dto.Exam.AddQuestionToExamDTO dto)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _examService.AddQuestionToExamAsync(examId, dto.QuestionId, dto.Points, dto.Order, instructorId);
            return Ok(new { message = "Question added to exam" });
        }

        /// <summary>
        /// Remove a question from an exam.
        /// </summary>
        /// <param name="examId">The ID of the exam.</param>
        /// <param name="questionId">The ID of the question to remove.</param>
        /// <returns>A success message.</returns>
        [HttpDelete("{examId:int}/questions/{questionId:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> RemoveQuestion(int examId, int questionId)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _examService.RemoveQuestionFromExamAsync(examId, questionId, instructorId);
            return Ok(new { message = "Question removed from exam" });
        }

        /// <summary>
        /// Retrieve aggregated results for all students in a specific exam.
        /// </summary>
        /// <param name="examId">The ID of the exam.</param>
        /// <returns>A list of student results for the exam.</returns>
        [HttpGet("{examId:int}/results")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetResultsForExam(int examId)
        {
            var instructorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var results = await _examService.GetExamResultsForInstructorAsync(examId, instructorId);
            return Ok(results);
        }
    }
}
