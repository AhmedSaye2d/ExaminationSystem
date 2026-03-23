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

        Task<IEnumerable<Exam.Application.Dto.Exam.ExamDTO>> GetCourseExamsAsync(int courseId, int userId, string role);
        Task<IEnumerable<Exam.Application.Dto.Student.StudentDTO>> GetCourseStudentsAsync(int courseId);
        Task AssignInstructorToCourseAsync(int courseId, int instructorId);
    }
}