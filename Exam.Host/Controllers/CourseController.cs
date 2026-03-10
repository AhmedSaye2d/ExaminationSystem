using Exam.Application.Dto.Course;
using Exam.Application.Services.Interfaces.ICourseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers
{
    [ApiController]
    [Route("api/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _courseService.GetAllAsync();
            return Ok(res);
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _courseService.GetByIdAsync(id);
            return Ok(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CourseCreateDTO dto)
        {
            await _courseService.CreateAsync(dto);
            return Ok(new { message = "Course created successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CourseCreateDTO dto)
        {
            await _courseService.UpdateAsync(id, dto);
            return Ok(new { message = "Course updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseService.DeleteAsync(id);
            return Ok(new { message = "Course deleted successfully" });
        }

        [HttpGet("{courseId:int}/exams")]
        public async Task<IActionResult> GetCourseExams(int courseId)
        {
            var exams = await _courseService.GetCourseExamsAsync(courseId);
            return Ok(exams);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{courseId:int}/assign-instructor")]
        public async Task<IActionResult> AssignInstructor(int courseId, [FromQuery] int instructorId)
        {
            await _courseService.AssignInstructorToCourseAsync(courseId, instructorId);
            return Ok(new { message = "Instructor assigned successfully" });
        }
    }
}
