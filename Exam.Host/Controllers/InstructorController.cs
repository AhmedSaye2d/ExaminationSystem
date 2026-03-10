using Exam.Application.Dto.Instructor;
using Exam.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Exam.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/instructors")]
    public class InstructorsController : ControllerBase
    {
        private readonly IInstructorService _instructorService;

        public InstructorsController(IInstructorService instructorService)
        {
            _instructorService = instructorService;
        }

        /// <summary>
        /// Retrieve all instructors.
        /// </summary>
        /// <returns>A list of instructors.</returns>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _instructorService.GetAllAsync();
            return Ok(res);
        }

        /// <summary>
        /// Retrieve an instructor by ID.
        /// </summary>
        /// <param name="id">Instructor ID.</param>
        /// <returns>The requested instructor details.</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _instructorService.GetByIdAsync(id);
            return Ok(res);
        }

        /// <summary>
        /// [Admin Only] Create a new instructor account.
        /// </summary>
        /// <param name="dto">Instructor creation data.</param>
        /// <returns>Result of the creation process.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] InstructorCreateDTO dto)
        {
            var res = await _instructorService.CreateAsync(dto);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// [Admin Only] Update an existing instructor's profile.
        /// </summary>
        /// <param name="id">Instructor ID to update.</param>
        /// <param name="dto">Updated instructor data.</param>
        /// <returns>Result of the update process.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] InstructorUpdateDTO dto)
        {
            var res = await _instructorService.UpdateAsync(id, dto);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        /// <summary>
        /// [Admin Only] Delete an instructor by ID.
        /// </summary>
        /// <param name="id">Instructor ID to delete.</param>
        /// <returns>Result of the deletion process.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _instructorService.DeleteAsync(id);
            return res.Success ? Ok(res) : BadRequest(res);
        }
    }
}
