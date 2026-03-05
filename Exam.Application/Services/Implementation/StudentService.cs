using AutoMapper;
using Exam.Application.Dto.Common;
using Exam.Application.Dto.Course;
using Exam.Application.Dto.Student;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Enum;
using Exam.Domain.Interface;

namespace Exam.Application.Services.Implementation
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ================================
        // GET ALL (Only Active Students)
        // ================================
        public async Task<IEnumerable<StudentDTO>> GetAllAsync()
        {
            var students = await _unitOfWork.Repository<Student>().GetAllAsync();

            var activeStudents = students
                .Where(s => !s.IsDeleted);

            return _mapper.Map<IEnumerable<StudentDTO>>(activeStudents);
        }

        // ================================
        // GET BY ID
        // ================================
        public async Task<StudentDTO> GetByIdAsync(int id)
        {
            var student = await _unitOfWork.Repository<Student>().GetByIdAsync(id);

            if (student == null || student.IsDeleted)
                throw new ItemNotFoundException("Student not found");

            return _mapper.Map<StudentDTO>(student);
        }

        // ================================
        // CREATE
        // ================================
        public async Task<ServiceResponse> CreateAsync(StudentCreateDTO dto)
        {
            var departmentExists = await _unitOfWork.Repository<Department>().ExistsAsync(dto.MajorId);
            if (!departmentExists)
                throw new ItemNotFoundException("Department not found");

            var student = _mapper.Map<Student>(dto);

            student.UserType = UserType.Student;
            student.IsActive = true;
            student.IsDeleted = false;

            await _unitOfWork.Repository<Student>().AddAsync(student);
            await _unitOfWork.CompleteAsync();

            return ServiceResponse.Ok("Student created successfully");
        }

        // ================================
        // UPDATE
        // ================================
        public async Task<ServiceResponse> UpdateAsync(int id, StudentUpdateDTO dto)
        {
            var studentRepo = _unitOfWork.Repository<Student>();
            var student = await studentRepo.GetByIdAsync(id);

            if (student == null || student.IsDeleted)
                throw new ItemNotFoundException("Student not found");

            var departmentExists = await _unitOfWork.Repository<Department>().ExistsAsync(dto.MajorId);
            if (!departmentExists)
                throw new ItemNotFoundException("Department not found");

            _mapper.Map(dto, student);

            await studentRepo.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            return ServiceResponse.Ok("Student updated successfully");
        }

        // ================================
        // DELETE (Soft Delete)
        // ================================
        public async Task<ServiceResponse> DeleteAsync(int id)
        {
            var studentRepo = _unitOfWork.Repository<Student>();
            var student = await studentRepo.GetByIdAsync(id);

            if (student == null || student.IsDeleted)
                throw new ItemNotFoundException("Student not found");

            var enrollments = await _unitOfWork.Repository<CourseStudent>()
                .FindAsync(cs => cs.StudentId == id);

            if (enrollments.Any())
                return ServiceResponse.Fail("Cannot delete student with enrolled courses");

            student.IsDeleted = true;
            student.IsActive = false;

            await studentRepo.UpdateAsync(student);
            await _unitOfWork.CompleteAsync();

            return ServiceResponse.Ok("Student deleted successfully");
        }

        // ================================
        // GET STUDENT COURSES
        // ================================
        public async Task<IEnumerable<CourseDTO>> GetStudentCoursesAsync(int studentId)
        {
            var student = await _unitOfWork.Repository<Student>().GetByIdAsync(studentId);

            if (student == null || student.IsDeleted)
                throw new ItemNotFoundException("Student not found");

            var enrollments = await _unitOfWork.Repository<CourseStudent>()
                .FindAsync(cs => cs.StudentId == studentId);

            if (!enrollments.Any())
                return Enumerable.Empty<CourseDTO>();

            var courseIds = enrollments
                .Select(e => e.CourseId);

            var courses = await _unitOfWork.Repository<Course>()
                .FindAsync(c => courseIds.Contains(c.Id));

            return _mapper.Map<IEnumerable<CourseDTO>>(courses);
        }
    }
}