using Exam.Application.Dto.Common;
using Exam.Application.Dto.Course;
using Exam.Application.Dto.Student;

public interface IStudentService
{
    Task<IEnumerable<StudentDTO>> GetAllAsync();

    Task<StudentDTO> GetByIdAsync(int id); // مش nullable

    Task<ServiceResponse> CreateAsync(StudentCreateDTO dto);

    Task<ServiceResponse> UpdateAsync(int id, StudentUpdateDTO dto);

    Task<ServiceResponse> DeleteAsync(int id);

    Task<IEnumerable<CourseDTO>> GetStudentCoursesAsync(int studentId);
}