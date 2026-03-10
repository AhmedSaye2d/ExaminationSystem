using Exam.Application.Dto.Common;
using Exam.Application.Dto.Course;
using Exam.Application.Dto.Exam;
using Exam.Application.Dto.Student;
using Exam.Application.Dto.SubmitExam;

public interface IStudentService
{
    Task<IEnumerable<StudentDTO>> GetAllAsync();

    Task<StudentDTO> GetByIdAsync(int id); // مش nullable

    Task<ServiceResponse> CreateAsync(StudentCreateDTO dto);

    Task<ServiceResponse> UpdateAsync(int id, StudentUpdateDTO dto);

    Task<ServiceResponse> DeleteAsync(int id);

    Task<IEnumerable<CourseDTO>> GetStudentCoursesAsync(int studentId);
    Task<IEnumerable<ExamDTO>> GetStudentExamsAsync(int studentId);
    Task<IEnumerable<ExamResultDTO>> GetStudentResultsAsync(int studentId);
    Task<ServiceResponse> EnrollCourseAsync(int studentId, int courseId);
}