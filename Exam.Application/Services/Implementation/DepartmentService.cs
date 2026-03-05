using AutoMapper;
using Exam.Application.Dto.Department;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IDepartmentServices;
using Exam.Domain;
using Exam.Domain.Entities;
namespace Exam.Application.Services.Implementation;

public class DepartmentService : IDepartmentService
{
    private readonly IGenericRepository<Department> _repo;
    private readonly IMapper _mapper;

    public DepartmentService(IGenericRepository<Department> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DepartmentDTO>> GetAllAsync()
    {
        var departments = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<DepartmentDTO>>(departments);
    }

    public async Task<DepartmentDTO> GetByIdAsync(int id)
    {
        var department = await _repo.GetByIdAsync(id);

        if (department == null)
            throw new ItemNotFoundException("Department not found");

        return _mapper.Map<DepartmentDTO>(department);
    }

    public async Task CreateAsync(DepartmentCreateDTO dto)
    {
        var existing = await _repo.FindAsync(d => d.Name == dto.Name);

        if (existing.Any())
            throw new ArgumentException("Department already exists");

        var department = _mapper.Map<Department>(dto);

        await _repo.AddAsync(department);
    }

    public async Task UpdateAsync(int id, DepartmentCreateDTO dto)
    {
        var department = await _repo.GetByIdAsync(id);

        if (department == null)
            throw new ItemNotFoundException("Department not found");

        var existing = await _repo
            .FindAsync(d => d.Name == dto.Name && d.Id != id);

        if (existing.Any())
            throw new ArgumentException("Department already exists");

        _mapper.Map(dto, department);

        await _repo.UpdateAsync(department);
    }

    public async Task DeleteAsync(int id)
    {
        var department = await _repo.GetByIdAsync(id);

        if (department == null)
            throw new ItemNotFoundException("Department not found");

        await _repo.DeleteAsync(id);
    }
}