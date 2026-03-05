using Exam.Application.Dto.Course;
using Exam.Application.Services.Interfaces.ICourseService;
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

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CourseCreateDTO dto)
        {
            await _courseService.CreateAsync(dto);
            return Ok(new { message = "Course created successfully" });
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CourseCreateDTO dto)
        {
            await _courseService.UpdateAsync(id, dto);
            return Ok(new { message = "Course updated successfully" });
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseService.DeleteAsync(id);
            return Ok(new { message = "Course deleted successfully" });
        }
    }
}
