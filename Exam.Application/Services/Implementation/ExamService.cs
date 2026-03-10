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
            if (exam.InstructorID != instructorId)
                throw new UnauthorizedAccessException("You are not authorized to view stats for this exam");

            var enrollments = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(es => es.ExamId == examId);

            return new ExamStatsDTO
            {
                ExamId = exam.Id,
                ExamTitle = exam.Name,
                TotalStudents = enrollments.Count(),
                SubmittedCount = enrollments.Count(es => es.IsSubmitted),
                AverageScore = enrollments.Any(es => es.IsSubmitted) 
                    ? enrollments.Where(es => es.IsSubmitted).Average(es => es.Score) 
                    : 0
            };
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

        public async Task UpdateAsync(int id, ExamCreateDTO dto, int instructorId)
        {
            var examRepo = _unitOfWork.Repository<Domain.Entities.Exam>();

            var exam = await examRepo.GetByIdAsync(id);
            if (exam == null || exam.IsDeleted)
                throw new ItemNotFoundException("Exam not found");

            // Check Ownership
            if (exam.InstructorID != instructorId)
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
            if (exam.InstructorID != instructorId)
                throw new UnauthorizedAccessException("You can only delete your own exams");

            exam.IsDeleted = true;
            await examRepo.UpdateAsync(exam);

            await _unitOfWork.CompleteAsync();
        }
    }
}
