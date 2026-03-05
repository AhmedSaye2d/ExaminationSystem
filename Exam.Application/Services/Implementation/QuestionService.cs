using AutoMapper;
using Exam.Application.Dto.Question;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IQuestionServices;
using Exam.Domain;
using Exam.Domain.Entities;

public class QuestionService : IQuestionService
{
    private readonly IGenericRepository<Question> _questionRepo;
    private readonly IGenericRepository<Choice> _choiceRepo;
    private readonly IMapper _mapper;

    public QuestionService(
        IGenericRepository<Question> questionRepo,
        IGenericRepository<Choice> choiceRepo,
        IMapper mapper)
    {
        _questionRepo = questionRepo;
        _choiceRepo = choiceRepo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<QuestionDTO>> GetAllAsync()
    {
        var questions = await _questionRepo.GetAllAsync();
        return _mapper.Map<IEnumerable<QuestionDTO>>(questions);
    }

    public async Task<QuestionDTO> GetByIdAsync(int id)
    {
        var question = await _questionRepo.GetByIdAsync(id)
                       ?? throw new ItemNotFoundException("Question not found");

        return _mapper.Map<QuestionDTO>(question);
    }

    public async Task CreateAsync(QuestionCreateDTO dto)
    {
        var question = _mapper.Map<Question>(dto);

        await _questionRepo.AddAsync(question);
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

        await _questionRepo.AddAsync(question);

        foreach (var choiceDto in dto.Choices)
        {
            await _choiceRepo.AddAsync(new Choice
            {
                Text = choiceDto.Text,
                IsCorrectAnswer = choiceDto.IsCorrect,
                QuestionId = question.Id
            });
        }

        return question.Id;
    }

    public async Task UpdateAsync(int id, QuestionCreateDTO dto)
    {
        var question = await _questionRepo.GetByIdAsync(id)
                       ?? throw new ItemNotFoundException("Question not found");

        _mapper.Map(dto, question);

        await _questionRepo.UpdateAsync(question);
    }

    public async Task DeleteAsync(int id)
    {
        var question = await _questionRepo.GetByIdAsync(id)
                       ?? throw new ItemNotFoundException("Question not found");

        await _questionRepo.DeleteAsync(id);
    }
}