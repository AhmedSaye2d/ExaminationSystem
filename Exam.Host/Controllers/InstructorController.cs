using Exam.Application.Dto.Instructor;
using Exam.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers
{
    [ApiController]
    [Route("api/instructors")]
    public class InstructorsController : ControllerBase
    {
        private readonly IInstructorService _instructorService;

        public InstructorsController(IInstructorService instructorService)
        {
            _instructorService = instructorService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _instructorService.GetAllAsync();
            return Ok(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _instructorService.GetByIdAsync(id);
            return Ok(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] InstructorCreateDTO dto)
        {
            var res = await _instructorService.CreateAsync(dto);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] InstructorUpdateDTO dto)
        {
            var res = await _instructorService.UpdateAsync(id, dto);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _instructorService.DeleteAsync(id);
            return res.Success ? Ok(res) : BadRequest(res);
        }
    }
}
