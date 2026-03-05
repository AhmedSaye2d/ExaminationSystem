using Exam.Application.Dto.Common;
using Exam.Application.Dto.Instructor;

namespace Exam.Application.Services.Interfaces
{
    
        public interface IInstructorService
        {
            Task<IEnumerable<InstructorReadDTO>> GetAllAsync();
            Task<InstructorReadDTO> GetByIdAsync(int id);
            Task<ServiceResponse> CreateAsync(InstructorCreateDTO dto);
            Task<ServiceResponse> UpdateAsync(int id, InstructorUpdateDTO dto);
            Task<ServiceResponse> DeleteAsync(int id);
        }
    }
