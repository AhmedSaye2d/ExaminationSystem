using Exam.Application.Dto.Choice;

namespace Exam.Application.Services.Interfaces.IChoiceServices
{
    public interface IChoiceService
    {
        Task<IEnumerable<ChoiceDTO>> GetAllAsync();

        Task<ChoiceDTO> GetByIdAsync(int id);

        Task CreateAsync(int questionId, ChoiceCreateDTO dto);

        Task AddRangeAsync(int questionId, IEnumerable<ChoiceCreateDTO> choices);

        Task UpdateAsync(int id, ChoiceCreateDTO dto);

        Task DeleteAsync(int id);

        Task DeleteRangeAsync(IEnumerable<int> ids);
    }
}