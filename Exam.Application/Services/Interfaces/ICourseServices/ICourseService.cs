using Exam.Application.Dto.Course;

namespace Exam.Application.Services.Interfaces.ICourseService
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDTO>> GetAllAsync();

        Task<CourseDTO> GetByIdAsync(int id);

        Task CreateAsync(CourseCreateDTO dto);

        Task UpdateAsync(int id, CourseCreateDTO dto);

        Task DeleteAsync(int id);
    }
}