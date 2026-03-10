using AutoMapper;
using Exam.Application.Dto.Common;
using Exam.Application.Dto.Instructor;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Enum;
using Exam.Domain.Interface;
using Exam.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Exam.Application.Services.Implementation
{
    public class InstructorService : IInstructorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public InstructorService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
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

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return ServiceResponse.Fail("Email is already in use");

            var instructor = _mapper.Map<Instructor>(dto);

            instructor.UserName = dto.Email;
            instructor.HireDate = dto.HireDate ?? DateTime.UtcNow;
            instructor.UserType = UserType.Instructor;
            instructor.IsActive = true;
            instructor.IsDeleted = false;

            // Use Identity UserManager
            var result = await _userManager.CreateAsync(instructor, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResponse.Fail(errors);
            }

            // Assign to Instructor Role
            await _userManager.AddToRoleAsync(instructor, UserType.Instructor.ToString());

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