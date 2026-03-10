using Exam.Application.Dto.Department;
using Exam.Application.Services.Interfaces.IDepartmentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Exam.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/departments")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        /// <summary>
        /// Retrieve all academic departments.
        /// </summary>
        /// <returns>A list of departments.</returns>
        [AllowAnonymous]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _departmentService.GetAllAsync();
            return Ok(res);
        }

        /// <summary>
        /// Retrieve a department by ID.
        /// </summary>
        /// <param name="id">Department ID.</param>
        /// <returns>The requested department details.</returns>
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _departmentService.GetByIdAsync(id);
            return Ok(res);
        }

        /// <summary>
        /// [Admin Only] Create a new department.
        /// </summary>
        /// <param name="dto">Department details.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] DepartmentCreateDTO dto)
        {
            await _departmentService.CreateAsync(dto);
            return Ok(new { message = "Department created successfully" });
        }

        /// <summary>
        /// [Admin Only] Update an existing department.
        /// </summary>
        /// <param name="id">Department ID to update.</param>
        /// <param name="dto">Updated department details.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentCreateDTO dto)
        {
            await _departmentService.UpdateAsync(id, dto);
            return Ok(new { message = "Department updated successfully" });
        }

        /// <summary>
        /// [Admin Only] Delete a department by ID.
        /// </summary>
        /// <param name="id">Department ID to delete.</param>
        /// <returns>Success message.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _departmentService.DeleteAsync(id);
            return Ok(new { message = "Department deleted successfully" });
        }
    }
}
