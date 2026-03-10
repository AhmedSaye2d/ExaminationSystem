using AutoMapper;
using Exam.Application.Dto.Common;
using Exam.Application.Dto.Course;
using Exam.Application.Dto.Exam;
using Exam.Application.Dto.Student;
using Exam.Application.Dto.SubmitExam;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Entities.Identity;
using Exam.Domain.Enum;
using Exam.Domain.Interface;
using Microsoft.AspNetCore.Identity;

namespace Exam.Application.Services.Implementation
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        // ================================
        // GET ALL (Only Active Students)
        // ================================
        public async Task<IEnumerable<StudentDTO>> GetAllAsync()
        {
            // جلب كل الطلاب النشطين مع تضمين الكورسات المسجلين فيها
            var students = await _unitOfWork.Repository<Student>()
                .FindAsync(s => !s.IsDeleted, "CourseStudents.Course");

            return _mapper.Map<IEnumerable<StudentDTO>>(students);
        }

        // ================================
        // GET BY ID
        // ================================
        public async Task<StudentDTO> GetByIdAsync(int id)
        {
            var studentRepo = _unitOfWork.Repository<Student>();
            
            // جلب الطالب مع تحميل الكورسات المرتبطة به باستخدام Include
            var students = await studentRepo.FindAsync(
                s => s.Id == id && !s.IsDeleted, 
                "CourseStudents.Course");

            var student = students.FirstOrDefault();

            if (student == null)
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

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return ServiceResponse.Fail("Email is already in use");

            var student = _mapper.Map<Student>(dto);
            student.UserName = dto.Email;
            student.UserType = UserType.Student;
            student.IsActive = true;
            student.IsDeleted = false;

            // Use Identity UserManager to create user (this hashes the password)
            var result = await _userManager.CreateAsync(student, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResponse.Fail(errors);
            }

            // Assign to Student Role
            await _userManager.AddToRoleAsync(student, UserType.Student.ToString());

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
            var studentRepo = _unitOfWork.Repository<Student>();
            
            // جلب الطالب مع تحميل الكورسات المرتبطة به
            var studentSessions = await studentRepo.FindAsync(
                s => s.Id == studentId && !s.IsDeleted, 
                "CourseStudents.Course");

            var student = studentSessions.FirstOrDefault();

            if (student == null)
                throw new ItemNotFoundException("Student not found");

            // استخلاص الكورسات من جدول الربط مع التأكد أنها غير محذوفة
            var courses = student.CourseStudents
                .Where(cs => cs.Course != null && !cs.Course.IsDeleted)
                .Select(cs => cs.Course);

            return _mapper.Map<IEnumerable<CourseDTO>>(courses);
        }

        public async Task<ServiceResponse> EnrollCourseAsync(int studentId, int courseId)
        {
            var studentExists = await _unitOfWork.Repository<Student>().ExistsAsync(studentId);
            if (!studentExists)
                throw new ItemNotFoundException("Student not found");

            var courseExists = await _unitOfWork.Repository<Course>().ExistsAsync(courseId);
            if (!courseExists)
                throw new ItemNotFoundException("Course not found");

            var enrollmentRepo = _unitOfWork.Repository<CourseStudent>();
            
            // التأكد من أن الطالب لم يسجل في هذا الكورس مسبقاً
            var existingEnrollment = await enrollmentRepo.FindAsync(
                cs => cs.StudentId == studentId && cs.CourseId == courseId);

            if (existingEnrollment.Any())
                return ServiceResponse.Fail("Student is already enrolled in this course");

            await enrollmentRepo.AddAsync(new CourseStudent
            {
                StudentId = studentId,
                CourseId = courseId
            });

            await _unitOfWork.CompleteAsync();

            return ServiceResponse.Ok("Student enrolled in course successfully");
        }

        public async Task<IEnumerable<ExamDTO>> GetStudentExamsAsync(int studentId)
        {
            // Get IDs of courses the student is enrolled in
            var enrollments = await _unitOfWork.Repository<CourseStudent>()
                .FindAsync(cs => cs.StudentId == studentId);
            
            var courseIds = enrollments.Select(cs => cs.CourseId).ToList();

            // Get published exams for these courses
            var exams = await _unitOfWork.Repository<Domain.Entities.Exam>()
                .FindAsync(e => courseIds.Contains(e.CourseID) && e.IsPublished && !e.IsDeleted);

            return _mapper.Map<IEnumerable<ExamDTO>>(exams);
        }

        public async Task<IEnumerable<ExamResultDTO>> GetStudentResultsAsync(int studentId)
        {
            var results = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(es => es.StudentId == studentId, "Exam,Student");

            return _mapper.Map<IEnumerable<ExamResultDTO>>(results);
        }
    }
}