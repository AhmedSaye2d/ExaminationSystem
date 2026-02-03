using System.Collections.Generic;
using System.Threading.Tasks;

namespace Exam.Application.Services.Interfaces.Base
{
    public interface IBaseService<TEntity, TDto> where TEntity : class
    {
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<TDto> GetByIdAsync(int id);
        Task<TDto> AddAsync(TDto dto);
        Task<bool> UpdateAsync(TDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
