using AutoMapper;
using Exam.Application.Dto.Course;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.ICourseService;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Interface;

namespace Exam.Application.Services.Implementation
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseDTO>> GetAllAsync()
        {
            var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
            return _mapper.Map<IEnumerable<CourseDTO>>(courses);
        }

        public async Task<CourseDTO> GetByIdAsync(int id)
        {
            var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id);

            if (course == null)
                throw new ItemNotFoundException("Course not found");

            return _mapper.Map<CourseDTO>(course);
        }

        public async Task CreateAsync(CourseCreateDTO dto)
        {
            var courseRepo = _unitOfWork.Repository<Course>();
            var departmentRepo = _unitOfWork.Repository<Department>();

            // تأكد إن القسم موجود
            var departmentExists = await departmentRepo.ExistsAsync(dto.DepartmentId);
            if (!departmentExists)
                throw new ItemNotFoundException("Department not found");

            // منع تكرار اسم الكورس داخل نفس القسم
            var existingCourse = await courseRepo
                .FindAsync(c => c.Name == dto.Name && c.DepartmentId == dto.DepartmentId);

            if (existingCourse.Any())
                throw new ArgumentException("Course already exists in this department");

            var course = _mapper.Map<Course>(dto);

            await courseRepo.AddAsync(course);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(int id, CourseCreateDTO dto)
        {
            var courseRepo = _unitOfWork.Repository<Course>();
            var departmentRepo = _unitOfWork.Repository<Department>();

            var course = await courseRepo.GetByIdAsync(id);
            if (course == null)
                throw new ItemNotFoundException("Course not found");

            var departmentExists = await departmentRepo.ExistsAsync(dto.DepartmentId);
            if (!departmentExists)
                throw new ItemNotFoundException("Department not found");

            var existingCourse = await courseRepo
                .FindAsync(c => c.Name == dto.Name
                             && c.DepartmentId == dto.DepartmentId
                             && c.Id != id);

            if (existingCourse.Any())
                throw new ArgumentException("Course already exists in this department");

            _mapper.Map(dto, course);

            await courseRepo.UpdateAsync(course);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var courseRepo = _unitOfWork.Repository<Course>();
            var course = await courseRepo.GetByIdAsync(id);
            if (course == null || course.IsDeleted)
                throw new ItemNotFoundException("Course not found");

            // Check for existing exams
            var exams = await _unitOfWork.Repository<Domain.Entities.Exam>().FindAsync(e => e.CourseID == id && !e.IsDeleted);
            if (exams.Any())
                throw new ArgumentException("Cannot delete course with active exams");

            // Check for student enrollments
            var enrollments = await _unitOfWork.Repository<CourseStudent>().FindAsync(cs => cs.CourseId == id);
            if (enrollments.Any())
                throw new ArgumentException("Cannot delete course with enrolled students");

            course.IsDeleted = true;
            await courseRepo.UpdateAsync(course);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<Exam.Application.Dto.Exam.ExamDTO>> GetCourseExamsAsync(int courseId)
        {
            var exams = await _unitOfWork.Repository<Domain.Entities.Exam>()
                .FindAsync(e => e.CourseID == courseId);

            return _mapper.Map<IEnumerable<Exam.Application.Dto.Exam.ExamDTO>>(exams);
        }

        public async Task AssignInstructorToCourseAsync(int courseId, int instructorId)
        {
            var courseRepo = _unitOfWork.Repository<Course>();
            var instructorRepo = _unitOfWork.Repository<Instructor>();
            var courseInstructorRepo = _unitOfWork.Repository<CourseInstructor>();

            var courseExists = await courseRepo.ExistsAsync(courseId);
            if (!courseExists) throw new ItemNotFoundException("Course not found");

            var instructorExists = await instructorRepo.ExistsAsync(instructorId);
            if (!instructorExists) throw new ItemNotFoundException("Instructor not found");

            var existingAssignment = await courseInstructorRepo
                .FindAsync(ci => ci.CourseId == courseId && ci.InstructorId == instructorId);

            if (existingAssignment.Any())
                throw new ArgumentException("Instructor is already assigned to this course");

            await courseInstructorRepo.AddAsync(new CourseInstructor
            {
                CourseId = courseId,
                InstructorId = instructorId
            });

            await _unitOfWork.CompleteAsync();
        }
    }
}