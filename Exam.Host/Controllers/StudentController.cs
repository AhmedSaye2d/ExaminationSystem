using Exam.Application.Dto.Student;
using Exam.Application.Dto.Course;
using Exam.Application.Services.Interfaces;
using Exam.Application.Services.Interfaces.IStudentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exam.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Retrieve all registered students.
        /// </summary>
        /// <returns>A list of students.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _studentService.GetAllAsync();
            return Ok(res);
        }

        /// <summary>
        /// Retrieve a paged list of students.
        /// </summary>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var res = await _studentService.GetPagedAsync(page, pageSize, search);
            return Ok(new { data = res.Items, totalCount = res.TotalCount, page, pageSize });
        }

        /// <summary>
        /// Get a student's profile by ID.
        /// </summary>
        /// <param name="id">Student ID.</param>
        /// <returns>Student profile details.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _studentService.GetByIdAsync(id);
            return Ok(res);
        }

        /// <summary>
        /// [Admin Only] Create a new student record.
        /// </summary>
        /// <param name="dto">Student creation data.</param>
        /// <returns>Result of the creation process.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] StudentCreateDTO dto)
        {
            var res = await _studentService.CreateAsync(dto);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// [Admin Only] Update an existing student's profile.
        /// </summary>
        /// <param name="id">Student ID to update.</param>
        /// <param name="dto">Updated student data.</param>
        /// <returns>Result of the update process.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDTO dto)
        {
            var res = await _studentService.UpdateAsync(id, dto);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// [Admin Only] Delete a student record by ID.
        /// </summary>
        /// <param name="id">Student ID to delete.</param>
        /// <returns>Result of the deletion process.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _studentService.DeleteAsync(id);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Get the courses the currently authenticated student is enrolled in.
        /// </summary>
        /// <returns>A list of enrolled courses.</returns>
        [Authorize]
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _studentService.GetStudentCoursesAsync(studentId);
            return Ok(res);
        }

        /// <summary>
        /// Enroll the currently authenticated student in a specific course.
        /// </summary>
        /// <param name="courseId">The ID of the course to enroll in.</param>
        /// <returns>Result of the enrollment process.</returns>
        [Authorize]
        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollInCourse([FromQuery] int courseId)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _studentService.EnrollCourseAsync(studentId, courseId);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// Get the exams available for the currently authenticated student.
        /// </summary>
        /// <returns>A list of exams.</returns>
        [Authorize(Roles = "Student")]
        [HttpGet("my-exams")]
        public async Task<IActionResult> GetMyExams()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _studentService.GetStudentExamsAsync(studentId);
            return Ok(res);
        }

        /// <summary>
        /// Get the exam results for the currently authenticated student.
        /// </summary>
        /// <returns>A list of exam results.</returns>
        [Authorize(Roles = "Student")]
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _studentService.GetStudentResultsAsync(studentId);
            return Ok(res);
        }

        /// <summary>
        /// Get all courses that a specific student is enrolled in.
        /// </summary>
        /// <param name="id">Student ID.</param>
        /// <returns>A list of enrolled courses.</returns>
        [HttpGet("{id:int}/courses")]
        public async Task<IActionResult> GetStudentCourses(int id)
        {
            var res = await _studentService.GetStudentCoursesAsync(id);
            return Ok(res);
        }

        /// <summary>
        /// [Admin/Instructor View] Get all exam results for a specific student.
        /// </summary>
        /// <param name="id">Student ID.</param>
        /// <returns>A list of exam results.</returns>
        [Authorize(Roles = "Admin,Instructor")]
        [HttpGet("{id:int}/results")]
        public async Task<IActionResult> GetStudentResults(int id)
        {
            var res = await _studentService.GetStudentResultsAsync(id);
            return Ok(res);
        }
    }
}