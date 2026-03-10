using Exam.Application.Dto.Student;
using Exam.Application.Dto.Course;
using Exam.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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

        [Authorize(Roles = "Admin")]
        /// <summary>
        /// Retrieve all registered students.
        /// </summary>
        /// <returns>A list of students.</returns>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _studentService.GetAllAsync();
            return Ok(res);
        }

        [Authorize(Roles = "Admin")]
        /// <summary>
        /// Get a student's profile by ID.
        /// </summary>
        /// <param name="id">Student ID.</param>
        /// <returns>Student profile details.</returns>
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _studentService.GetByIdAsync(id);
            return Ok(res);
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize]
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _studentService.GetStudentCoursesAsync(studentId);
            return Ok(res);
        }

        [Authorize]
        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollInCourse([FromQuery] int courseId)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _studentService.EnrollCourseAsync(studentId, courseId);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("my-exams")]
        public async Task<IActionResult> GetMyExams()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _studentService.GetStudentExamsAsync(studentId);
            return Ok(res);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        /// <summary>
        /// Get all courses that a specific student is enrolled in.
        /// </summary>
        /// <param name="id">Student ID.</param>
        /// <returns>A list of enrolled courses.</returns>
        [HttpGet("{id:int}/courses")]
        public async Task<IActionResult> GetStudentCourses(int id)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _studentService.GetStudentResultsAsync(studentId);
            return Ok(res);
        }
    }
}