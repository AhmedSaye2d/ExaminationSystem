using AutoMapper;
using Exam.Application.Dto.Exam;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IExamServices;
using Exam.Domain.Entities;
using Exam.Domain.Interface;

namespace Exam.Application.Services.Implementation
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ExamService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ExamDTO>> GetAllAsync()
        {
            var exams = await _unitOfWork
                .Repository<Domain.Entities.Exam>()
                .GetAllAsync();

            return _mapper.Map<IEnumerable<ExamDTO>>(exams);
        }

        public async Task<ExamDTO> GetByIdAsync(int id)
        {
            var exam = await _unitOfWork
                .Repository<Domain.Entities.Exam>()
                .GetByIdAsync(id);

            if (exam == null)
                throw new ItemNotFoundException("Exam not found");

            return _mapper.Map<ExamDTO>(exam);
        }

        public async Task CreateAsync(ExamCreateDTO dto)
        {
            // Validate Course exists
            var course = await _unitOfWork.Repository<Course>().GetByIdAsync(dto.CourseId);
            if (course == null)
                throw new ItemNotFoundException($"Course with ID {dto.CourseId} not found");

            // Validate Instructor exists
            var instructor = await _unitOfWork.Repository<Instructor>().GetByIdAsync(dto.InstructorId);
            if (instructor == null)
                throw new ItemNotFoundException($"Instructor with ID {dto.InstructorId} not found");

            var exam = _mapper.Map<Domain.Entities.Exam>(dto);

            await _unitOfWork
                .Repository<Domain.Entities.Exam>()
                .AddAsync(exam);

            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(int id, ExamCreateDTO dto)
        {
            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();

            var exam = await examRepo.GetByIdAsync(id);
            if (exam == null)
                throw new ItemNotFoundException("Exam not found");

            _mapper.Map(dto, exam);

            await examRepo.UpdateAsync(exam);

            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();

            var exam = await examRepo.GetByIdAsync(id);
            if (exam == null)
                throw new ItemNotFoundException("Exam not found");

            await examRepo.DeleteAsync(id);

            await _unitOfWork.CompleteAsync();
        }

        public async Task AddQuestionsToExamAsync(int examId, IEnumerable<int> questionIds)
        {
            if (questionIds == null || !questionIds.Any())
                throw new ArgumentException("No questions provided");

            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();
            var questionRepo = _unitOfWork.Repository<Question>();
            var examQuestionRepo = _unitOfWork.Repository<ExamQuestion>();

            var exam = await examRepo.GetByIdAsync(examId);
            if (exam == null)
                throw new ItemNotFoundException("Exam not found");

            var questions = await questionRepo
                .FindAsync(q => questionIds.Contains(q.Id));

            if (!questions.Any())
                throw new ItemNotFoundException("No valid questions found");

            var existingExamQuestions = await examQuestionRepo
                .FindAsync(eq => eq.ExamId == examId);

            var existingQuestionIds = existingExamQuestions
                .Select(eq => eq.QuestionId)
                .ToHashSet();

            int addedCount = 0;

            foreach (var question in questions)
            {
                if (existingQuestionIds.Contains(question.Id))
                    continue;

                await examQuestionRepo.AddAsync(new ExamQuestion
                {
                    ExamId = examId,
                    QuestionId = question.Id,
                    Points = question.Grade
                });

                addedCount++;
            }

            if (addedCount == 0)
                throw new ArgumentException("All questions already added to exam");

            // إعادة حساب الدرجة
            var examQuestions = await _unitOfWork.Repository<ExamQuestion>()
                .FindAsync(x => x.ExamId == examId);

            var examQuestionList = examQuestions.ToList();

            var questionIdsInExam = examQuestionList.Select(q => q.QuestionId).ToList();

            exam.TotalGrade = examQuestionList.Sum(eq => eq.Points); // Use the ToList() result here

            await examRepo.UpdateAsync(exam);

            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0)
                throw new Exception("Failed to add questions");
        }

        public async Task RemoveQuestionFromExamAsync(int examId, int questionId)
        {
            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();
            var examQuestionRepo = _unitOfWork.Repository<ExamQuestion>();

            var exam = await examRepo.GetByIdAsync(examId)
                       ?? throw new ItemNotFoundException("Exam not found");

            var examQuestions = await examQuestionRepo.FindAsync(eq => eq.ExamId == examId && eq.QuestionId == questionId);
            var examQuestion = examQuestions.FirstOrDefault()
                               ?? throw new ItemNotFoundException("Question not found in this exam");

            await examQuestionRepo.DeleteAsync(examQuestion.Id);

            exam.TotalGrade -= examQuestion.Points;
            if (exam.TotalGrade < 0) exam.TotalGrade = 0;

            await examRepo.UpdateAsync(exam);
            await _unitOfWork.CompleteAsync();
        }
    }
}