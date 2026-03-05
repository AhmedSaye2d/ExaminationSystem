using AutoMapper;
using Exam.Application.Dto.Common;
using Exam.Application.Dto.Instructor;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Enum;

namespace Exam.Application.Services.Implementation
{
    public class InstructorService : IInstructorService
    {
        private readonly IGenericRepository<Instructor> _instructorRepo;
        private readonly IGenericRepository<Department> _departmentRepo;
        private readonly IMapper _mapper;

        public InstructorService(
            IGenericRepository<Instructor> instructorRepo,
            IGenericRepository<Department> departmentRepo,
            IMapper mapper)
        {
            _instructorRepo = instructorRepo;
            _departmentRepo = departmentRepo;
            _mapper = mapper;
        }

        // ================================
        // GET ALL
        // ================================
        public async Task<IEnumerable<InstructorReadDTO>> GetAllAsync()
        {
            var instructors = await _instructorRepo.GetAllAsync();

            var active = instructors.Where(i => !i.IsDeleted);

            return _mapper.Map<IEnumerable<InstructorReadDTO>>(active);
        }

        // ================================
        // GET BY ID
        // ================================
        public async Task<InstructorReadDTO> GetByIdAsync(int id)
        {
            var instructor = await _instructorRepo.GetByIdAsync(id);

            if (instructor == null || instructor.IsDeleted)
                throw new ItemNotFoundException("Instructor not found");

            return _mapper.Map<InstructorReadDTO>(instructor);
        }

        // ================================
        // CREATE
        // ================================
        public async Task<ServiceResponse> CreateAsync(InstructorCreateDTO dto)
        {
            var departmentExists = await _departmentRepo.ExistsAsync(dto.DepartmentId);
            if (!departmentExists)
                throw new ItemNotFoundException("Department not found");

            var instructor = _mapper.Map<Instructor>(dto);

            instructor.HireDate = dto.HireDate ?? DateTime.UtcNow;
            instructor.UserType = UserType.Instructor;

            await _instructorRepo.AddAsync(instructor);

            return ServiceResponse.Ok("Instructor created successfully");
        }

        // ================================
        // UPDATE
        // ================================
        public async Task<ServiceResponse> UpdateAsync(int id, InstructorUpdateDTO dto)
        {
            var instructor = await _instructorRepo.GetByIdAsync(id);

            if (instructor == null || instructor.IsDeleted)
                throw new ItemNotFoundException("Instructor not found");

            var departmentExists = await _departmentRepo.ExistsAsync(dto.DepartmentId);
            if (!departmentExists)
                throw new ItemNotFoundException("Department not found");

            _mapper.Map(dto, instructor);

            await _instructorRepo.UpdateAsync(instructor);

            return ServiceResponse.Ok("Instructor updated successfully");
        }

        // ================================
        // DELETE (Soft Delete)
        // ================================
        public async Task<ServiceResponse> DeleteAsync(int id)
        {
            var instructor = await _instructorRepo.GetByIdAsync(id);

            if (instructor == null || instructor.IsDeleted)
                throw new ItemNotFoundException("Instructor not found");

            instructor.IsDeleted = true;
            instructor.IsActive = false;

            await _instructorRepo.UpdateAsync(instructor);

            return ServiceResponse.Ok("Instructor deleted successfully");
        }
    }
}