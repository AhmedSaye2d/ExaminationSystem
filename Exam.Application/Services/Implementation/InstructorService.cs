using AutoMapper;
using Exam.Application.Dto.Common;
using Exam.Application.Dto.Instructor;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Enum;
using Exam.Domain.Interface;

namespace Exam.Application.Services.Implementation
{
    public class InstructorService : IInstructorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InstructorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ================================
        // GET ALL
        // ================================
        public async Task<IEnumerable<InstructorReadDTO>> GetAllAsync()
        {
            var instructors = await _unitOfWork.Repository<Instructor>().GetAllAsync();

            var active = instructors.Where(i => !i.IsDeleted);

            return _mapper.Map<IEnumerable<InstructorReadDTO>>(active);
        }

        // ================================
        // GET BY ID
        // ================================
        public async Task<InstructorReadDTO> GetByIdAsync(int id)
        {
            var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(id);

            if (instructor == null || instructor.IsDeleted)
                throw new ItemNotFoundException("Instructor not found");

            return _mapper.Map<InstructorReadDTO>(instructor);
        }

        // ================================
        // CREATE
        // ================================
        public async Task<ServiceResponse> CreateAsync(InstructorCreateDTO dto)
        {
            var departmentExists = await _unitOfWork.Repository<Department>().ExistsAsync(dto.DepartmentId);
            if (!departmentExists)
                throw new ItemNotFoundException("Department not found");

            var instructor = _mapper.Map<Instructor>(dto);

            instructor.HireDate = dto.HireDate ?? DateTime.UtcNow;
            instructor.UserType = UserType.Instructor;

            await _unitOfWork.Repository<Instructor>().AddAsync(instructor);
            await _unitOfWork.CompleteAsync();

            return ServiceResponse.Ok("Instructor created successfully");
        }

        // ================================
        // UPDATE
        // ================================
        public async Task<ServiceResponse> UpdateAsync(int id, InstructorUpdateDTO dto)
        {
            var instructorRepo = _unitOfWork.Repository<Instructor>();
            var instructor = await instructorRepo.GetByIdAsync(id);

            if (instructor == null || instructor.IsDeleted)
                throw new ItemNotFoundException("Instructor not found");

            var departmentExists = await _unitOfWork.Repository<Department>().ExistsAsync(dto.DepartmentId);
            if (!departmentExists)
                throw new ItemNotFoundException("Department not found");

            _mapper.Map(dto, instructor);

            await instructorRepo.UpdateAsync(instructor);
            await _unitOfWork.CompleteAsync();

            return ServiceResponse.Ok("Instructor updated successfully");
        }

        // ================================
        // DELETE (Soft Delete)
        // ================================
        public async Task<ServiceResponse> DeleteAsync(int id)
        {
            var instructorRepo = _unitOfWork.Repository<Instructor>();
            var instructor = await instructorRepo.GetByIdAsync(id);

            if (instructor == null || instructor.IsDeleted)
                throw new ItemNotFoundException("Instructor not found");

            instructor.IsDeleted = true;
            instructor.IsActive = false;

            await instructorRepo.UpdateAsync(instructor);
            await _unitOfWork.CompleteAsync();

            return ServiceResponse.Ok("Instructor deleted successfully");
        }
    }
}