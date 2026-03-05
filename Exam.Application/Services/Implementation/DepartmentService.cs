using AutoMapper;
using Exam.Application.Dto.Department;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IDepartmentServices;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Interface;
namespace Exam.Application.Services.Implementation;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DepartmentDTO>> GetAllAsync()
    {
        var departments = await _unitOfWork.Repository<Department>().GetAllAsync();
        return _mapper.Map<IEnumerable<DepartmentDTO>>(departments);
    }

    public async Task<DepartmentDTO> GetByIdAsync(int id)
    {
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(id);

        if (department == null)
            throw new ItemNotFoundException("Department not found");

        return _mapper.Map<DepartmentDTO>(department);
    }

    public async Task CreateAsync(DepartmentCreateDTO dto)
    {
        var repo = _unitOfWork.Repository<Department>();
        var existing = await repo.FindAsync(d => d.Name == dto.Name);

        if (existing.Any())
            throw new ArgumentException("Department already exists");

        var department = _mapper.Map<Department>(dto);

        await repo.AddAsync(department);
        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdateAsync(int id, DepartmentCreateDTO dto)
    {
        var repo = _unitOfWork.Repository<Department>();
        var department = await repo.GetByIdAsync(id);

        if (department == null)
            throw new ItemNotFoundException("Department not found");

        var existing = await repo
            .FindAsync(d => d.Name == dto.Name && d.Id != id);

        if (existing.Any())
            throw new ArgumentException("Department already exists");

        _mapper.Map(dto, department);

        await repo.UpdateAsync(department);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var repo = _unitOfWork.Repository<Department>();
        var department = await repo.GetByIdAsync(id);

        if (department == null)
            throw new ItemNotFoundException("Department not found");

        await repo.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();
    }
}