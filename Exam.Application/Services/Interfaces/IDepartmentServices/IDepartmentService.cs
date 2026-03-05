using Exam.Application.Dto.Department;

namespace Exam.Application.Services.Interfaces.IDepartmentServices
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDTO>> GetAllAsync();

        Task<DepartmentDTO> GetByIdAsync(int id);

        Task CreateAsync(DepartmentCreateDTO dto);

        Task UpdateAsync(int id, DepartmentCreateDTO dto);

        Task DeleteAsync(int id);
    }
}