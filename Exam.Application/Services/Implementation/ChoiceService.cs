using AutoMapper;
using Exam.Application.Dto.Choice;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IChoiceServices;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Interface;

namespace Exam.Application.Services.Implementation
{
    public class ChoiceService : IChoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChoiceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ChoiceDTO>> GetAllAsync()
        {
            var choices = await _unitOfWork.Repository<Choice>().GetAllAsync();
            return _mapper.Map<IEnumerable<ChoiceDTO>>(choices);
        }

        public async Task<ChoiceDTO> GetByIdAsync(int id)
        {
            var choice = await _unitOfWork.Repository<Choice>().GetByIdAsync(id);

            if (choice == null)
                throw new ItemNotFoundException("Choice not found");

            return _mapper.Map<ChoiceDTO>(choice);
        }

        public async Task CreateAsync(int questionId, ChoiceCreateDTO dto)
        {
            var questionRepo = _unitOfWork.Repository<Question>();
            var choiceRepo = _unitOfWork.Repository<Choice>();

            var questionExists = await questionRepo.ExistsAsync(questionId);
            if (!questionExists)
                throw new ItemNotFoundException("Question not found");

            // لو الإجابة الجديدة صحيحة → نلغي أي صحيحة قديمة
            if (dto.IsCorrect)
            {
                var existingCorrect = await choiceRepo
                    .FindAsync(c => c.QuestionId == questionId && c.IsCorrectAnswer);

                foreach (var c in existingCorrect)
                {
                    c.IsCorrectAnswer = false;
                    await choiceRepo.UpdateAsync(c);
                }
            }

            var choice = new Choice
            {
                Text = dto.Text,
                IsCorrectAnswer = dto.IsCorrect,
                QuestionId = questionId
            };

            await choiceRepo.AddAsync(choice);
            await _unitOfWork.CompleteAsync();
        }

        public async Task AddRangeAsync(int questionId, IEnumerable<ChoiceCreateDTO> dtos)
        {
            if (dtos == null || !dtos.Any())
                throw new ArgumentException("Choices list cannot be empty");

            var questionExists = await _unitOfWork.Repository<Question>().ExistsAsync(questionId);
            if (!questionExists)
                throw new ItemNotFoundException("Question not found");

            if (dtos.Count(c => c.IsCorrect) != 1)
                throw new ArgumentException("There must be exactly one correct answer");

            var choiceRepo = _unitOfWork.Repository<Choice>();
            foreach (var dto in dtos)
            {
                var choice = new Choice
                {
                    Text = dto.Text,
                    IsCorrectAnswer = dto.IsCorrect,
                    QuestionId = questionId
                };

                await choiceRepo.AddAsync(choice);
            }
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(int id, ChoiceCreateDTO dto)
        {
            var choiceRepo = _unitOfWork.Repository<Choice>();
            var choice = await choiceRepo.GetByIdAsync(id);
            if (choice == null)
                throw new ItemNotFoundException("Choice not found");

            if (dto.IsCorrect)
            {
                var existingCorrect = await choiceRepo
                    .FindAsync(c => c.QuestionId == choice.QuestionId && c.Id != id);

                foreach (var c in existingCorrect)
                {
                    c.IsCorrectAnswer = false;
                    await choiceRepo.UpdateAsync(c);
                }
            }

            choice.Text = dto.Text;
            choice.IsCorrectAnswer = dto.IsCorrect;

            await choiceRepo.UpdateAsync(choice);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var choiceRepo = _unitOfWork.Repository<Choice>();
            var exists = await choiceRepo.ExistsAsync(id);
            if (!exists)
                throw new ItemNotFoundException("Choice not found");

            await choiceRepo.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any())
                throw new ArgumentException("No ids provided");

            var choiceRepo = _unitOfWork.Repository<Choice>();
            foreach (var id in ids)
            {
                var exists = await choiceRepo.ExistsAsync(id);
                if (exists)
                    await choiceRepo.DeleteAsync(id);
            }
            await _unitOfWork.CompleteAsync();
        }
    }
}