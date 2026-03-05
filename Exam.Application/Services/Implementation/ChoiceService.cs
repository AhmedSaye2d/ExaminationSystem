using AutoMapper;
using Exam.Application.Dto.Choice;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IChoiceServices;
using Exam.Domain;
using Exam.Domain.Entities;

namespace Exam.Application.Services.Implementation
{
    public class ChoiceService : IChoiceService
    {
        private readonly IGenericRepository<Choice> _choiceRepo;
        private readonly IGenericRepository<Question> _questionRepo;
        private readonly IMapper _mapper;

        public ChoiceService(
            IGenericRepository<Choice> choiceRepo,
            IGenericRepository<Question> questionRepo,
            IMapper mapper)
        {
            _choiceRepo = choiceRepo;
            _questionRepo = questionRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ChoiceDTO>> GetAllAsync()
        {
            var choices = await _choiceRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<ChoiceDTO>>(choices);
        }

        public async Task<ChoiceDTO> GetByIdAsync(int id)
        {
            var choice = await _choiceRepo.GetByIdAsync(id);

            if (choice == null)
                throw new ItemNotFoundException("Choice not found");

            return _mapper.Map<ChoiceDTO>(choice);
        }

        public async Task CreateAsync(int questionId, ChoiceCreateDTO dto)
        {
            var questionExists = await _questionRepo.ExistsAsync(questionId);
            if (!questionExists)
                throw new ItemNotFoundException("Question not found");

            // لو الإجابة الجديدة صحيحة → نلغي أي صحيحة قديمة
            if (dto.IsCorrect)
            {
                var existingCorrect = await _choiceRepo
                    .FindAsync(c => c.QuestionId == questionId && c.IsCorrectAnswer);

                foreach (var c in existingCorrect)
                {
                    c.IsCorrectAnswer = false;
                    await _choiceRepo.UpdateAsync(c);
                }
            }

            var choice = new Choice
            {
                Text = dto.Text,
                IsCorrectAnswer = dto.IsCorrect,
                QuestionId = questionId
            };

            await _choiceRepo.AddAsync(choice);
        }

        public async Task AddRangeAsync(int questionId, IEnumerable<ChoiceCreateDTO> dtos)
        {
            if (dtos == null || !dtos.Any())
                throw new ArgumentException("Choices list cannot be empty");

            var questionExists = await _questionRepo.ExistsAsync(questionId);
            if (!questionExists)
                throw new ItemNotFoundException("Question not found");

            if (dtos.Count(c => c.IsCorrect) != 1)
                throw new ArgumentException("There must be exactly one correct answer");

            foreach (var dto in dtos)
            {
                var choice = new Choice
                {
                    Text = dto.Text,
                    IsCorrectAnswer = dto.IsCorrect,
                    QuestionId = questionId
                };

                await _choiceRepo.AddAsync(choice);
            }
        }

        public async Task UpdateAsync(int id, ChoiceCreateDTO dto)
        {
            var choice = await _choiceRepo.GetByIdAsync(id);
            if (choice == null)
                throw new ItemNotFoundException("Choice not found");

            if (dto.IsCorrect)
            {
                var existingCorrect = await _choiceRepo
                    .FindAsync(c => c.QuestionId == choice.QuestionId && c.Id != id);

                foreach (var c in existingCorrect)
                {
                    c.IsCorrectAnswer = false;
                    await _choiceRepo.UpdateAsync(c);
                }
            }

            choice.Text = dto.Text;
            choice.IsCorrectAnswer = dto.IsCorrect;

            await _choiceRepo.UpdateAsync(choice);
        }

        public async Task DeleteAsync(int id)
        {
            var exists = await _choiceRepo.ExistsAsync(id);
            if (!exists)
                throw new ItemNotFoundException("Choice not found");

            await _choiceRepo.DeleteAsync(id);
        }

        public async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any())
                throw new ArgumentException("No ids provided");

            foreach (var id in ids)
            {
                var exists = await _choiceRepo.ExistsAsync(id);
                if (exists)
                    await _choiceRepo.DeleteAsync(id);
            }
        }
    }
}