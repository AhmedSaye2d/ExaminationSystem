using Exam.Application.Dto.Student;
using Exam.Application.Dto.Course;
using Exam.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _studentService.GetAllAsync();
            return Ok(res);
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _studentService.GetByIdAsync(id);
            return Ok(res);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] StudentCreateDTO dto)
        {
            var res = await _studentService.CreateAsync(dto);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDTO dto)
        {
            var res = await _studentService.UpdateAsync(id, dto);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _studentService.DeleteAsync(id);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpGet("{id:int}/courses")]
        public async Task<IActionResult> GetStudentCourses(int id)
        {
            var res = await _studentService.GetStudentCoursesAsync(id);
            return Ok(res);
        }
    }
}