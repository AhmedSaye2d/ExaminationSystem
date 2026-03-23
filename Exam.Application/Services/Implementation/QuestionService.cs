using AutoMapper;
using Exam.Application.Dto.Question;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IQuestionServices;
using Exam.Domain.Entities;
using Exam.Domain.Interface;

namespace Exam.Application.Services.Implementation
{

public class QuestionService : IQuestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public QuestionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<QuestionDTO>> GetAllAsync()
    {
        var questions = await _unitOfWork.Repository<Question>().GetAllAsync();
        return _mapper.Map<IEnumerable<QuestionDTO>>(questions);
    }

    public async Task<(IEnumerable<QuestionDTO> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? courseId)
    {
        var (items, totalCount) = await _unitOfWork.Repository<Question>()
            .GetPagedAsync(
                page, 
                pageSize, 
                predicate: courseId.HasValue ? q => q.Exam != null && q.Exam.CourseID == courseId.Value : null
            );
            
        return (_mapper.Map<IEnumerable<QuestionDTO>>(items), totalCount);
    }

    public async Task<QuestionDTO> GetByIdAsync(int id)
    {
        var question = await _unitOfWork.Repository<Question>().GetByIdAsync(id)
                       ?? throw new ItemNotFoundException("Question not found");

        return _mapper.Map<QuestionDTO>(question);
    }

    public async Task CreateAsync(QuestionCreateDTO dto)
    {
        var question = _mapper.Map<Question>(dto);
        await _unitOfWork.Repository<Question>().AddAsync(question);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<int> AddQuestionWithChoicesAsync(QuestionWithChoicesDTO dto)
    {
        // 1. Validation
        if (string.IsNullOrWhiteSpace(dto.Text))
            throw new ArgumentException("Question text cannot be empty");

        if (dto.Choices == null || !dto.Choices.Any())
            throw new ArgumentException("Question must contain choices");

        if (dto.Choices.Any(c => string.IsNullOrWhiteSpace(c.Text)))
            throw new ArgumentException("Choice text cannot be empty");

        if (dto.Choices.Count(c => c.IsCorrect) != 1)
            throw new ArgumentException("There must be exactly one correct answer");

        // 2. Mapping
        var question = _mapper.Map<Question>(dto);

        // 3. Save
        await _unitOfWork.Repository<Question>().AddAsync(question);
        await _unitOfWork.CompleteAsync();

        return question.Id;
    }

    public async Task UpdateAsync(int id, QuestionCreateDTO dto)
    {
        var questionRepo = _unitOfWork.Repository<Question>();
        var question = await questionRepo.GetByIdAsync(id)
                       ?? throw new ItemNotFoundException("Question not found");

        _mapper.Map(dto, question);

        await questionRepo.UpdateAsync(question);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var questionRepo = _unitOfWork.Repository<Question>();
        await questionRepo.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();
    }
}
}