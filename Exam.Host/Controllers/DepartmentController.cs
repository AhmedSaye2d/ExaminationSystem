using Exam.Application.Dto.Department;
using Exam.Application.Services.Interfaces.IDepartmentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers
{
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _departmentService.GetAllAsync();
            return Ok(res);
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _departmentService.GetByIdAsync(id);
            return Ok(res);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] DepartmentCreateDTO dto)
        {
            await _departmentService.CreateAsync(dto);
            return Ok(new { message = "Department created successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentCreateDTO dto)
        {
            await _departmentService.UpdateAsync(id, dto);
            return Ok(new { message = "Department updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _departmentService.DeleteAsync(id);
            return Ok(new { message = "Department deleted successfully" });
        }
    }
}
