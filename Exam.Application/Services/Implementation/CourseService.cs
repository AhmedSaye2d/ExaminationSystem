using AutoMapper;
using Exam.Application.Dto.Course;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.ICourseService;
using Exam.Domain;
using Exam.Domain.Entities;

namespace Exam.Application.Services.Implementation
{
    public class CourseService : ICourseService
    {
        private readonly IGenericRepository<Course> _courseRepo;
        private readonly IGenericRepository<Department> _departmentRepo;
        private readonly IMapper _mapper;

        public CourseService(
            IGenericRepository<Course> courseRepo,
            IGenericRepository<Department> departmentRepo,
            IMapper mapper)
        {
            _courseRepo = courseRepo;
            _departmentRepo = departmentRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseDTO>> GetAllAsync()
        {
            var courses = await _courseRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<CourseDTO>>(courses);
        }

        public async Task<CourseDTO> GetByIdAsync(int id)
        {
            var course = await _courseRepo.GetByIdAsync(id);

            if (course == null)
                throw new ItemNotFoundException("Course not found");

            return _mapper.Map<CourseDTO>(course);
        }

        public async Task CreateAsync(CourseCreateDTO dto)
        {
            // تأكد إن القسم موجود
            var departmentExists = await _departmentRepo.ExistsAsync(dto.DepartmentId);
            if (!departmentExists)
                throw new ItemNotFoundException("Department not found");

            // منع تكرار اسم الكورس داخل نفس القسم
            var existingCourse = await _courseRepo
                .FindAsync(c => c.Name == dto.Name && c.DepartmentId == dto.DepartmentId);

            if (existingCourse.Any())
                throw new ArgumentException("Course already exists in this department");

            var course = _mapper.Map<Course>(dto);

            await _courseRepo.AddAsync(course);
        }

        public async Task UpdateAsync(int id, CourseCreateDTO dto)
        {
            var course = await _courseRepo.GetByIdAsync(id);
            if (course == null)
                throw new ItemNotFoundException("Course not found");

            var departmentExists = await _departmentRepo.ExistsAsync(dto.DepartmentId);
            if (!departmentExists)
                throw new ItemNotFoundException("Department not found");

            var existingCourse = await _courseRepo
                .FindAsync(c => c.Name == dto.Name
                             && c.DepartmentId == dto.DepartmentId
                             && c.Id != id);

            if (existingCourse.Any())
                throw new ArgumentException("Course already exists in this department");

            _mapper.Map(dto, course);

            await _courseRepo.UpdateAsync(course);
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _courseRepo.GetByIdAsync(id);
            if (course == null)
                throw new ItemNotFoundException("Course not found");

            await _courseRepo.DeleteAsync(id);
        }
    }
}