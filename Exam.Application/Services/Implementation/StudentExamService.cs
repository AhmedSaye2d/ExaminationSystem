using AutoMapper;
using Exam.Application.Dto.Exam;
using Exam.Application.Dto.Question;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces;
using Exam.Application.Services.Interfaces.IExamStudentServices;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Exam.Domain.Enum;

using Microsoft.Extensions.Logging;

namespace Exam.Application.Services.Implementation
{
    public class StudentExamService : IStudentExamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentExamService> _logger;

        public StudentExamService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<StudentExamService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // ================================
        // START EXAM
        // ================================
        public async Task<int> StartExamAsync(int examId, int studentId)
        {
            var examRepo = _unitOfWork.Repository<Exam.Domain.Entities.Exam>();
            var studentRepo = _unitOfWork.Repository<Student>();
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var courseStudentRepo = _unitOfWork.Repository<CourseStudent>();

            var exam = await examRepo.GetByIdAsync(examId)
                       ?? throw new ItemNotFoundException("Exam not found");

            // 1. SECURE: Check if exam is published
            if (!exam.IsPublished)
                throw new UnauthorizedAccessException("This exam is not available yet");

            var studentExists = await studentRepo.ExistsAsync(studentId);
            if (!studentExists)
                throw new ItemNotFoundException("Student not found");

            // 2. SECURE: Check if student is enrolled in the course
            var enrollments = await courseStudentRepo.FindAsync(x => x.StudentId == studentId && x.CourseId == exam.CourseID);
            if (!enrollments.Any())
                throw new UnauthorizedAccessException("Student is not enrolled in this course");

            // 3. SECURE: Check timing
            var now = DateTime.UtcNow;
            if (now < exam.StartDate)
                throw new ArgumentException($"Exam has not started yet. Starts at {exam.StartDate}");
            if (now > exam.DueDate)
                throw new ArgumentException("Exam date has passed");

            // 4. SECURE: Check if already submitted or has existing session
            var existingSessions = await examStudentRepo.FindAsync(x => x.ExamId == examId && x.StudentId == studentId);
            var existing = existingSessions.FirstOrDefault();

            if (existing != null)
            {
                if (existing.Status == ExamStatus.Submitted || existing.Status == ExamStatus.Expired)
                    throw new ArgumentException("You have already completed this exam");

                return existing.Id; // Return existing session ID to continue
            }

            var examStudent = new ExamStudent
            {
                ExamId = examId,
                StudentId = studentId,
                StartDate = now,
                Status = ExamStatus.InProgress
            };

            await examStudentRepo.AddAsync(examStudent);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Student {StudentId} started exam {ExamId}. Session ID: {SessionId}", studentId, examId, examStudent.Id);

            return examStudent.Id;
        }

        // ================================
        // SAVE ANSWER
        // ================================
        public async Task SaveAnswerAsync(int examStudentId, int studentId, int questionId, int choiceId)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var examAnswerRepo = _unitOfWork.Repository<ExamAnswer>();

            var examStudent = await examStudentRepo.FindAsync(x => x.Id == examStudentId, "Exam")
                                .ContinueWith(t => t.Result.FirstOrDefault())
                                ?? throw new ItemNotFoundException("Exam session not found");

            // 1. SECURE: Check ownership
            if (examStudent.StudentId != studentId)
                throw new UnauthorizedAccessException("This session does not belong to you");

            if (examStudent.Status == ExamStatus.Submitted)
                throw new ArgumentException("Exam already submitted");

            // 2. SECURE: Check timeout
            var exam = examStudent.Exam;
            if (exam?.Settings != null)
            {
                var elapsedMinutes = (DateTime.UtcNow - examStudent.StartDate).TotalMinutes;
                if (elapsedMinutes > exam.Settings.DurationMinutes + 2) // 2 minutes grace period
                    throw new ArgumentException("Exam time has expired");
            }

            var choice = await _unitOfWork.Repository<Choice>().GetByIdAsync(choiceId)
                        ?? throw new ItemNotFoundException("Invalid choice");

            var question = await _unitOfWork.Repository<Question>().GetByIdAsync(questionId)
                ?? throw new ItemNotFoundException("Question not found in exam");

            if (question.ExamId != examStudent.ExamId)
                throw new ArgumentException("Question does not belong to this exam");

            if (choice.QuestionId != questionId)
                throw new ArgumentException("Choice does not belong to this question");

            var existingAnswer = await examAnswerRepo
                .FindAsync(x => x.ExamStudentId == examStudentId
                             && x.QuestionId == questionId);

            if (existingAnswer.Any())
            {
                var answer = existingAnswer.First();
                answer.ChoiceId = choiceId;
                await examAnswerRepo.UpdateAsync(answer);
            }
            else
            {
                await examAnswerRepo.AddAsync(new ExamAnswer
                {
                    ExamStudentId = examStudentId,
                    StudentId = examStudent.StudentId,
                    ExamId = examStudent.ExamId,
                    QuestionId = questionId,
                    ChoiceId = choiceId
                });
            }

            await _unitOfWork.CompleteAsync();
        }

        // ================================
        // SUBMIT EXAM
        // ================================
        public async Task SubmitExamAsync(int examStudentId, int studentId)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var examAnswerRepo = _unitOfWork.Repository<ExamAnswer>();
            var questionRepo = _unitOfWork.Repository<Question>();
            var choiceRepo = _unitOfWork.Repository<Choice>();

            var examStudent = await examStudentRepo.FindAsync(x => x.Id == examStudentId, "Exam")
                                .ContinueWith(t => t.Result.FirstOrDefault())
                                ?? throw new ItemNotFoundException("Exam session not found");

            // 1. SECURE: Check ownership
            if (examStudent.StudentId != studentId)
                throw new UnauthorizedAccessException("This session does not belong to you");

            if (examStudent.Status == ExamStatus.Submitted)
                throw new ArgumentException("Exam already submitted");

            var answers = await examAnswerRepo
                .FindAsync(x => x.ExamStudentId == examStudentId);

            if (!answers.Any())
            {
                // If no answers, score is 0 and mark as submitted
                examStudent.Score = 0;
                examStudent.Status = ExamStatus.Submitted;
                examStudent.SubmissionDate = DateTime.UtcNow;
                examStudent.IsPassed = false;
                await examStudentRepo.UpdateAsync(examStudent);
                await _unitOfWork.CompleteAsync();
                return;
            }

            var choiceIds = answers.Select(a => a.ChoiceId).ToList();
            var choices = (await choiceRepo.FindAsync(c => choiceIds.Contains(c.Id))).ToList();

            var questions = (await questionRepo
                .FindAsync(x => x.ExamId == examStudent.ExamId)).ToList();

            double totalScore = 0;

            foreach (var answer in answers)
            {
                var choice = choices.FirstOrDefault(c => c.Id == answer.ChoiceId);

                if (choice?.IsCorrectAnswer == true)
                {
                    var question = questions
                        .FirstOrDefault(q => q.Id == answer.QuestionId);

                    if (question != null)
                        totalScore += question.Grade;
                }
            }

            examStudent.Score = totalScore;
            examStudent.Status = ExamStatus.Submitted;
            examStudent.SubmissionDate = DateTime.UtcNow;
            
            // Fix: Include Exam property was added in the FindAsync above
            examStudent.IsPassed = examStudent.Exam != null && examStudent.Score >= examStudent.Exam.PassingScore;

            await examStudentRepo.UpdateAsync(examStudent);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Student {StudentId} submitted exam {ExamId}. Score: {Score}/{Total}", examStudent.StudentId, examStudent.ExamId, examStudent.Score, examStudent.Exam?.TotalGrade);
        }

        // ================================
        // GET QUESTIONS FOR SESSION (SECURE)
        // ================================
        public async Task<IEnumerable<Exam.Application.Dto.Question.QuestionForStudentDTO>> GetExamQuestionsAsync(int examStudentId, int studentId)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var questionRepo = _unitOfWork.Repository<Exam.Domain.Entities.Question>();

            var examStudent = await examStudentRepo.FindAsync(x => x.Id == examStudentId, "Exam")
                .ContinueWith(t => t.Result.FirstOrDefault())
                ?? throw new ItemNotFoundException("Exam session not found");

            // 1. SECURE: Check ownership
            if (examStudent.StudentId != studentId)
                throw new UnauthorizedAccessException("This session does not belong to you");

            if (examStudent.Status == ExamStatus.Submitted)
                throw new ArgumentException("Exam already submitted");

            // Fetch Questions for this exam directly
            var questions = await questionRepo.FindAsync(q => q.ExamId == examStudent.ExamId, "Choices");

            var questionDtos = _mapper.Map<List<Exam.Application.Dto.Question.QuestionForStudentDTO>>(questions);

            // Apply Shuffling using session ID as seed for consistency
            var settings = examStudent.Exam?.Settings;
            if (settings != null)
            {
                // Use session ID as seed so order doesn't change on refresh
                var rng = new Random(examStudentId); 
                
                if (settings.ShuffleQuestions)
                {
                    questionDtos = questionDtos.OrderBy(x => rng.Next()).ToList();
                }

                if (settings.ShuffleChoices)
                {
                    foreach (var qDto in questionDtos)
                    {
                        qDto.Choices = qDto.Choices.OrderBy(x => rng.Next()).ToList();
                    }
                }
            }

            return questionDtos;
        }

        // ================================
        // GET EXAM RESULTS
        // ================================
        public async Task<Exam.Application.Dto.SubmitExam.ExamResultDTO> GetExamResultAsync(int examId, int studentId)
        {
            var examStudents = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(x => x.ExamId == examId && x.StudentId == studentId, "Exam", "Student");

            var session = examStudents.FirstOrDefault() ?? throw new ItemNotFoundException("Exam session not found");

            return new Exam.Application.Dto.SubmitExam.ExamResultDTO
            {
                ExamId = session.ExamId,
                StudentId = session.StudentId,
                ExamName = session.Exam?.Name ?? "Unknown",
                StudentName = session.Student != null ? $"{session.Student.FirstName} {session.Student.LastName}" : "Unknown",
                Score = session.Score,
                TotalGrade = session.Exam?.TotalGrade ?? 0,
                IsPassed = session.Exam != null && session.Score >= session.Exam.PassingScore,
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.Status == ExamStatus.Submitted
            };
        }

        public async Task<Exam.Application.Dto.SubmitExam.ExamResultDTO> GetResultBySessionAsync(int examStudentId, int studentId)
        {
            var examStudents = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(x => x.Id == examStudentId, "Exam", "Student");

            var session = examStudents.FirstOrDefault() ?? throw new ItemNotFoundException("Exam session not found");

            // 1. SECURE: Check ownership if not admin/instructor (though this method is mostly for students)
            if (session.StudentId != studentId)
                throw new UnauthorizedAccessException("This result does not belong to you");

            return new Exam.Application.Dto.SubmitExam.ExamResultDTO
            {
                ExamId = session.ExamId,
                StudentId = session.StudentId,
                ExamName = session.Exam?.Name ?? "Unknown",
                StudentName = session.Student != null ? $"{session.Student.FirstName} {session.Student.LastName}" : "Unknown",
                Score = session.Score,
                TotalGrade = session.Exam?.TotalGrade ?? 0,
                IsPassed = session.Exam != null && session.Score >= session.Exam.PassingScore,
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.Status == ExamStatus.Submitted
            };
        }

        public async Task<IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO>> GetStudentResultsAsync(int studentId)
        {
            var examStudents = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(x => x.StudentId == studentId, "Exam", "Student");

            return examStudents.Select(session => new Exam.Application.Dto.SubmitExam.ExamResultDTO
            {
                ExamId = session.ExamId,
                StudentId = session.StudentId,
                ExamName = session.Exam?.Name ?? "Unknown",
                StudentName = session.Student != null ? $"{session.Student.FirstName} {session.Student.LastName}" : "Unknown",
                Score = session.Score,
                TotalGrade = session.Exam?.TotalGrade ?? 0,
                IsPassed = session.Exam != null && session.Score >= session.Exam.PassingScore,
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.Status == ExamStatus.Submitted
            });
        }

        public async Task<(IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO> Items, int TotalCount)> GetStudentResultsPagedAsync(int studentId, int page, int pageSize)
        {
            var (items, totalCount) = await _unitOfWork.Repository<ExamStudent>()
                .GetPagedAsync(page, pageSize, x => x.StudentId == studentId, true, "Exam", "Student");

            var dtos = items.Select(session => new Exam.Application.Dto.SubmitExam.ExamResultDTO
            {
                ExamId = session.ExamId,
                StudentId = session.StudentId,
                ExamName = session.Exam?.Name ?? "Unknown",
                StudentName = session.Student != null ? $"{session.Student.FirstName} {session.Student.LastName}" : "Unknown",
                Score = session.Score,
                TotalGrade = session.Exam?.TotalGrade ?? 0,
                IsPassed = session.Exam != null && session.Score >= session.Exam.PassingScore,
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.Status == ExamStatus.Submitted
            });

            return (dtos, totalCount);
        }

        public async Task<IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO>> GetExamResultsAsync(int examId)
        {
            var examStudents = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(x => x.ExamId == examId, "Exam", "Student");

            return examStudents.Select(session => new Exam.Application.Dto.SubmitExam.ExamResultDTO
            {
                ExamId = session.ExamId,
                StudentId = session.StudentId,
                ExamName = session.Exam?.Name ?? "Unknown",
                StudentName = session.Student != null ? $"{session.Student.FirstName} {session.Student.LastName}" : "Unknown",
                Score = session.Score,
                TotalGrade = session.Exam?.TotalGrade ?? 0,
                IsPassed = session.Exam != null && session.Score >= session.Exam.PassingScore,
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.Status == ExamStatus.Submitted
            });
        }

        public async Task<(IEnumerable<Exam.Application.Dto.SubmitExam.ExamResultDTO> Items, int TotalCount)> GetExamResultsPagedAsync(int examId, int page, int pageSize)
        {
            var (items, totalCount) = await _unitOfWork.Repository<ExamStudent>()
                .GetPagedAsync(page, pageSize, x => x.ExamId == examId, true, "Exam", "Student");

            var dtos = items.Select(session => new Exam.Application.Dto.SubmitExam.ExamResultDTO
            {
                ExamId = session.ExamId,
                StudentId = session.StudentId,
                ExamName = session.Exam?.Name ?? "Unknown",
                StudentName = session.Student != null ? $"{session.Student.FirstName} {session.Student.LastName}" : "Unknown",
                Score = session.Score,
                TotalGrade = session.Exam?.TotalGrade ?? 0,
                IsPassed = session.Exam != null && session.Score >= session.Exam.PassingScore,
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.Status == ExamStatus.Submitted
            });

            return (dtos, totalCount);
        }

        public async Task<Exam.Application.Dto.SubmitExam.ResumeExamDTO> ResumeExamAsync(int examStudentId, int studentId)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var examAnswerRepo = _unitOfWork.Repository<ExamAnswer>();
            var questionRepo = _unitOfWork.Repository<Question>();

            var session = await examStudentRepo.GetByIdAsync(examStudentId) ?? throw new ItemNotFoundException("Exam session not found");

            // 1. SECURE: Check ownership
            if (session.StudentId != studentId)
                throw new UnauthorizedAccessException("This session does not belong to you");

            if (session.Status == ExamStatus.Submitted || session.Status == ExamStatus.Expired)
                throw new ArgumentException("This exam session has already ended and cannot be resumed");

            var exam = await _unitOfWork.Repository<Domain.Entities.Exam>().GetByIdAsync(session.ExamId);

            // 2. SECURE: Check timeout
            if (exam?.Settings != null)
            {
                var elapsedMinutes = (DateTime.UtcNow - session.StartDate).TotalMinutes;
                if (elapsedMinutes > exam.Settings.DurationMinutes)
                {
                   _logger.LogWarning("Student {StudentId} tried to resume expired exam session {SessionId}", studentId, examStudentId);
                   throw new ArgumentException("The exam time has expired");
                }
            }

            // Questions
            var questions = await questionRepo.FindAsync(q => q.ExamId == session.ExamId, "Choices");

            // Saved answers
            var answers = await examAnswerRepo.FindAsync(a => a.ExamStudentId == examStudentId);

            var dto = new Exam.Application.Dto.SubmitExam.ResumeExamDTO
            {
                ExamStudentId = examStudentId,
                ExamId = session.ExamId,
                Questions = _mapper.Map<IEnumerable<Exam.Application.Dto.Question.QuestionForStudentDTO>>(questions),
                SavedAnswers = answers.Select(a => new Exam.Application.Dto.SubmitExam.ExamAnswerDTO { QuestionId = a.QuestionId, ChoiceId = a.ChoiceId }),
                RemainingMinutes = exam != null && exam.Settings != null ? Math.Max(0, exam.Settings.DurationMinutes - (int)(DateTime.UtcNow - session.StartDate).TotalMinutes) : 0
            };

            return dto;
        }
    }
}