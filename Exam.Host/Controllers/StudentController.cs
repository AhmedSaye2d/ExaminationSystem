using Exam.Application.Dto.Student;
using Exam.Application.Dto.Course;
using Exam.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exam.API.Controllers
{
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
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _studentService.GetAllAsync();
            return Ok(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _studentService.GetByIdAsync(id);
            return Ok(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] StudentCreateDTO dto)
        {
            var res = await _studentService.CreateAsync(dto);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDTO dto)
        {
            var res = await _studentService.UpdateAsync(id, dto);
            return res.Success ? Ok(res) : BadRequest(res);
        }

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
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var res = await _studentService.GetStudentResultsAsync(studentId);
            return Ok(res);
        }
    }
}