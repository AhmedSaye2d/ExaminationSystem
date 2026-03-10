using Exam.Application.Dto.Choice;
using Exam.Application.Services.Interfaces.IChoiceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Ensure this is present for [Authorize]

namespace Exam.Host.Controllers
{
    [Authorize] // This is already present. The instruction implies adding it if missing, or adding role restrictions.
    [ApiController]
    [Route("api/choices")] // Keep original route
    public class ChoiceController : ControllerBase // Keep original class name
    {
        private readonly IChoiceService _choiceService;

        public ChoiceController(IChoiceService choiceService) // Keep original constructor name
        {
            _choiceService = choiceService;
        }

        /// <summary>
        /// Retrieve all question choices.
        /// </summary>
        /// <returns>A list of choices.</returns>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _choiceService.GetAllAsync();
            return Ok(res);
        }

        /// <summary>
        /// Retrieve a specific choice by its ID.
        /// </summary>
        /// <param name="id">Choice ID.</param>
        /// <returns>The requested choice.</returns>
        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _choiceService.GetByIdAsync(id);
            return Ok(res);
        }

        /// <summary>
        /// Create a new choice for a specific question.
        /// </summary>
        /// <param name="questionId">Target question ID.</param>
        /// <param name="dto">Choice details.</param>
        /// <returns>Success message.</returns>
        [HttpPost("Create/{questionId:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Create(int questionId, [FromBody] ChoiceCreateDTO dto)
        {
            await _choiceService.CreateAsync(questionId, dto);
            return Ok(new { message = "Choice created successfully" });
        }

        /// <summary>
        /// Add multiple choices to a question at once.
        /// </summary>
        /// <param name="questionId">Target question ID.</param>
        /// <param name="choices">List of choices to add.</param>
        /// <returns>Success message.</returns>
        [HttpPost("AddRange/{questionId:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> AddRange(int questionId, [FromBody] IEnumerable<ChoiceCreateDTO> choices)
        {
            await _choiceService.AddRangeAsync(questionId, choices);
            return Ok(new { message = "Choices added successfully" });
        }

        /// <summary>
        /// Update an existing choice's information.
        /// </summary>
        /// <param name="id">Choice ID to update.</param>
        /// <param name="dto">Updated choice details.</param>
        /// <returns>Success message.</returns>
        [HttpPut("Update/{id:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ChoiceCreateDTO dto)
        {
            await _choiceService.UpdateAsync(id, dto);
            return Ok(new { message = "Choice updated successfully" });
        }

        /// <summary>
        /// Delete a single choice by ID.
        /// </summary>
        /// <param name="id">Choice ID to delete.</param>
        /// <returns>Success message.</returns>
        [HttpDelete("Delete/{id:int}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _choiceService.DeleteAsync(id);
            return Ok(new { message = "Choice deleted successfully" });
        }

        /// <summary>
        /// Bulk delete a range of choices by their IDs.
        /// </summary>
        /// <param name="ids">List of IDs to delete.</param>
        /// <returns>Success message.</returns>
        [HttpDelete("DeleteRange")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> DeleteRange([FromBody] IEnumerable<int> ids)
        {
            await _choiceService.DeleteRangeAsync(ids);
            return Ok(new { message = "Choices deleted successfully" });
        }
    }
}
