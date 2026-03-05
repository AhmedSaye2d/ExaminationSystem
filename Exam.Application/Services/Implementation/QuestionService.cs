using AutoMapper;
using Exam.Application.Dto.Question;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IQuestionServices;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Interface;

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
                predicate: courseId.HasValue ? q => q.ExamQuestions.Any(eq => eq.Exam.CourseID == courseId.Value) : null
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
        if (dto.Choices == null || !dto.Choices.Any())
            throw new ArgumentException("Question must contain choices");

        if (dto.Choices.Count(c => c.IsCorrect) != 1)
            throw new ArgumentException("There must be exactly one correct answer");

        var question = new Question
        {
            Text = dto.Text,
            Grade = dto.Grade,
            Type = dto.Type
        };

        var questionRepo = _unitOfWork.Repository<Question>();
        var choiceRepo = _unitOfWork.Repository<Choice>();

        await questionRepo.AddAsync(question);

        foreach (var choiceDto in dto.Choices)
        {
            await choiceRepo.AddAsync(new Choice
            {
                Text = choiceDto.Text,
                IsCorrectAnswer = choiceDto.IsCorrect,
                QuestionId = question.Id
            });
        }

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
        var question = await questionRepo.GetByIdAsync(id)
                       ?? throw new ItemNotFoundException("Question not found");

        await questionRepo.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();
    }
}