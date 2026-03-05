using Exam.Application.Dto.Choice;
using Exam.Application.Services.Interfaces.IChoiceServices;
using Microsoft.AspNetCore.Mvc;

namespace Exam.API.Controllers
{
    [ApiController]
    [Route("api/choices")]
    public class ChoiceController : ControllerBase
    {
        private readonly IChoiceService _choiceService;

        public ChoiceController(IChoiceService choiceService)
        {
            _choiceService = choiceService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await _choiceService.GetAllAsync();
            return Ok(res);
        }

        [HttpGet("GetById/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var res = await _choiceService.GetByIdAsync(id);
            return Ok(res);
        }

        [HttpPost("Create/{questionId:int}")]
        public async Task<IActionResult> Create(int questionId, [FromBody] ChoiceCreateDTO dto)
        {
            await _choiceService.CreateAsync(questionId, dto);
            return Ok(new { message = "Choice created successfully" });
        }

        [HttpPost("AddRange/{questionId:int}")]
        public async Task<IActionResult> AddRange(int questionId, [FromBody] IEnumerable<ChoiceCreateDTO> choices)
        {
            await _choiceService.AddRangeAsync(questionId, choices);
            return Ok(new { message = "Choices added successfully" });
        }

        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ChoiceCreateDTO dto)
        {
            await _choiceService.UpdateAsync(id, dto);
            return Ok(new { message = "Choice updated successfully" });
        }

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _choiceService.DeleteAsync(id);
            return Ok(new { message = "Choice deleted successfully" });
        }

        [HttpDelete("DeleteRange")]
        public async Task<IActionResult> DeleteRange([FromBody] IEnumerable<int> ids)
        {
            await _choiceService.DeleteRangeAsync(ids);
            return Ok(new { message = "Choices deleted successfully" });
        }
    }
}
