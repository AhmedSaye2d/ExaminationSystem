using AutoMapper;
using Exam.Application.Dto.Exam;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IExamServices;
using Exam.Domain.Entities;
using Exam.Domain.Interface;
using Exam.Domain.Enum; // Added this using directive
using System.Linq; // Added this using directive
using System.Threading.Tasks;
using Exam.Application.Services.Interfaces; // Added this using directive
using FluentValidation;

namespace Exam.Application.Services.Implementation
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<ExamCreateDTO> _examValidator;

        public ExamService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IValidator<ExamCreateDTO> examValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _examValidator = examValidator;
        }

        public async Task<IEnumerable<ExamDTO>> GetAllAsync()
        {
            var exams = await _unitOfWork
                .Repository<Domain.Entities.Exam>()
                .GetAllAsync();

            return _mapper.Map<IEnumerable<ExamDTO>>(exams);
        }

        public async Task<(IEnumerable<ExamDTO> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, int? courseId = null)
        {
            var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.Exam>()
                .GetPagedAsync(
                    page,
                    pageSize,
                    predicate: courseId.HasValue ? e => e.CourseID == courseId.Value : null
                );

            return (_mapper.Map<IEnumerable<ExamDTO>>(items), totalCount);
        }

        public async Task<IEnumerable<ExamDTO>> GetInstructorExamsAsync(int instructorId)
        {
            var exams = await _unitOfWork
                .Repository<Domain.Entities.Exam>()
                .FindAsync(e => e.InstructorID == instructorId && !e.IsDeleted);

            return _mapper.Map<IEnumerable<ExamDTO>>(exams);
        }

        public async Task<ExamDTO> GetByIdAsync(int id)
        {
            var exam = await _unitOfWork
                .Repository<Domain.Entities.Exam>()
                .GetByIdAsync(id);

            if (exam == null || exam.IsDeleted)
                throw new ItemNotFoundException("Exam not found");

            return _mapper.Map<ExamDTO>(exam);
        }

        public async Task<ExamStatsDTO> GetExamStatsAsync(int examId, int instructorId)
        {
            var exam = await _unitOfWork.Repository<Domain.Entities.Exam>().GetByIdAsync(examId);

            if (exam == null || exam.IsDeleted)
                throw new ItemNotFoundException("Exam not found");

            // Security check: Only the owner or admin can see stats
            if (exam.InstructorID != instructorId && !_currentUserService.IsAdmin())
                throw new UnauthorizedAccessException("You are not authorized to view stats for this exam");

            var enrollments = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(es => es.ExamId == examId);

            return new ExamStatsDTO
            {
                ExamId = exam.Id,
                ExamTitle = exam.Name,
                TotalStudents = enrollments.Count(),
                SubmittedCount = enrollments.Count(es => es.Status == ExamStatus.Submitted),
                AverageScore = enrollments.Any(es => es.Status == ExamStatus.Submitted)
                    ? enrollments.Where(es => es.Status == ExamStatus.Submitted).Average(es => es.Score)
                    : 0
            };
        }

        public async Task CreateAsync(ExamCreateDTO dto)
        {
            var validation = await _examValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                throw new ArgumentException(validation.Errors.First().ErrorMessage);

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

        public async Task UpdateAsync(int id, ExamCreateDTO dto, int instructorId)
        {
            var validation = await _examValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                throw new ArgumentException(validation.Errors.First().ErrorMessage);

            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();
            
            var exam = await examRepo.GetByIdAsync(id);
            if (exam == null || exam.IsDeleted)
                throw new ItemNotFoundException("Exam not found");

            // Check Ownership
            if (exam.InstructorID != instructorId && !_currentUserService.IsAdmin())
                throw new UnauthorizedAccessException("You can only update your own exams");

            _mapper.Map(dto, exam);

            await examRepo.UpdateAsync(exam);

            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id, int instructorId)
        {
            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();

            var exam = await examRepo.GetByIdAsync(id);
            if (exam == null || exam.IsDeleted)
                throw new ItemNotFoundException("Exam not found");

            // Check Ownership
            if (exam.InstructorID != instructorId && !_currentUserService.IsAdmin())
                throw new UnauthorizedAccessException("You can only delete your own exams");
            // Check if there are student attempts
            var attempts = await _unitOfWork.Repository<ExamStudent>().FindAsync(x => x.ExamId == id && !x.IsDeleted);
            if (attempts.Any())
                throw new ArgumentException("Cannot delete exam with student attempts");

            exam.IsDeleted = true;
            await examRepo.UpdateAsync(exam);

            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<Exam.Application.Dto.Question.QuestionDTO>> GetQuestionsByExamIdAsync(int examId, int instructorId)
        {
            var exam = await _unitOfWork.Repository<Domain.Entities.Exam>().GetByIdAsync(examId) ?? throw new ItemNotFoundException("Exam not found");
            
            // 🔥 SECURE: Only the owner or admin can see the questions list through this management service
            if (exam.InstructorID != instructorId && !_currentUserService.IsAdmin())
                throw new UnauthorizedAccessException("You are not authorized to view questions for this exam");

            var questions = await _unitOfWork.Repository<Question>().FindAsync(q => q.ExamId == examId, "Choices");

            return _mapper.Map<IEnumerable<Exam.Application.Dto.Question.QuestionDTO>>(questions);
        }

        public async Task AddQuestionToExamAsync(int examId, int questionId, int points, int order, int instructorId)
        {
            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();
            var questionRepo = _unitOfWork.Repository<Question>();

            var exam = await examRepo.GetByIdAsync(examId) ?? throw new ItemNotFoundException("Exam not found");
            
            // 🔥 SECURE: Only the owner or admin can add questions to this exam
            if (exam.InstructorID != instructorId && !_currentUserService.IsAdmin())
                throw new UnauthorizedAccessException("You can only add questions to your own exams");

            var question = await questionRepo.GetByIdAsync(questionId) ?? throw new ItemNotFoundException("Question not found");

            // Ensure question is linked to this exam
            question.ExamId = examId;
            question.Grade = points;
            question.Order = order;

            await questionRepo.UpdateAsync(question);
            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveQuestionFromExamAsync(int examId, int questionId, int instructorId)
        {
            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();
            var questionRepo = _unitOfWork.Repository<Question>();

            var exam = await examRepo.GetByIdAsync(examId) ?? throw new ItemNotFoundException("Exam not found");

            // 🔥 SECURE: Only the owner or admin can remove questions from this exam
            if (exam.InstructorID != instructorId && !_currentUserService.IsAdmin())
                throw new UnauthorizedAccessException("You can only remove questions from your own exams");

            var question = await questionRepo.GetByIdAsync(questionId) ?? throw new ItemNotFoundException("Question not found");

            if (question.ExamId != examId)
                throw new ArgumentException("Question does not belong to the specified exam");

            // Detach question from exam (could delete or set ExamId = null depending on desired behavior)
            question.ExamId = null;
            await questionRepo.UpdateAsync(question);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ScheduleExamAsync(int id, ScheduleExamDTO dto, int instructorId)
        {
            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();
            var exam = await examRepo.GetByIdAsync(id) ?? throw new ItemNotFoundException("Exam not found");

            if (exam.InstructorID != instructorId && !_currentUserService.IsAdmin())
                throw new UnauthorizedAccessException("You can only schedule your own exams");

            exam.StartDate = dto.StartTime;
            exam.DueDate = dto.EndTime;
            exam.Settings ??= new Domain.Entities.ExamSettings();
            exam.Settings.DurationMinutes = dto.DurationMinutes;

            await examRepo.UpdateAsync(exam);
            await _unitOfWork.CompleteAsync();
        }

        public async Task PublishExamAsync(int id, int instructorId)
        {
            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();
            var exam = await examRepo.GetByIdAsync(id) ?? throw new ItemNotFoundException("Exam not found");

            if (exam.InstructorID != instructorId && !_currentUserService.IsAdmin())
                throw new UnauthorizedAccessException("You can only publish your own exams");

            exam.IsPublished = true;
            await examRepo.UpdateAsync(exam);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UnpublishExamAsync(int id, int instructorId)
        {
            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();
            var exam = await examRepo.GetByIdAsync(id) ?? throw new ItemNotFoundException("Exam not found");

            if (exam.InstructorID != instructorId && !_currentUserService.IsAdmin())
                throw new UnauthorizedAccessException("You can only unpublish your own exams");

            exam.IsPublished = false;
            await examRepo.UpdateAsync(exam);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<Exam.Application.Dto.Exam.InstructorExamResultDTO>> GetExamResultsForInstructorAsync(int examId, int instructorId)
        {
            var exam = await _unitOfWork.Repository<Domain.Entities.Exam>().GetByIdAsync(examId) ?? throw new ItemNotFoundException("Exam not found");

            if (exam.InstructorID != instructorId && !_currentUserService.IsAdmin())
                throw new UnauthorizedAccessException("You can only view results for your own exams");

            var examStudents = await _unitOfWork.Repository<ExamStudent>().FindAsync(es => es.ExamId == examId, "Student");

            return examStudents.Select(es => new Exam.Application.Dto.Exam.InstructorExamResultDTO
            {
                StudentId = es.StudentId,
                StudentName = es.Student != null ? es.Student.FirstName + " " + es.Student.LastName : string.Empty,
                Score = es.Score,
                Percentage = exam.TotalGrade > 0 ? (es.Score / exam.TotalGrade) * 100 : 0,
                SubmittedAt = es.SubmissionDate
            });
        }
    }
}
