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

namespace Exam.Application.Services.Implementation
{
    public class StudentExamService : IStudentExamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentExamService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

            var studentExists = await studentRepo.ExistsAsync(studentId);
            if (!studentExists)
                throw new ItemNotFoundException("Student not found");

            if (DateTime.UtcNow < exam.StartDate)
                throw new ArgumentException("Exam has not started yet");

            // 3. SECURE: Check if student is enrolled in the course
            var enrollments = await courseStudentRepo.FindAsync(x => x.StudentId == studentId && x.CourseId == exam.CourseID);
            if (!enrollments.Any())
                throw new UnauthorizedAccessException("Student is not enrolled in this course");

            // 4. SECURE: Check timing
            var now = DateTime.UtcNow;
            if (now < exam.StartDate)
                throw new ArgumentException($"Exam has not started yet. Starts at {exam.StartDate}");
            if (now > exam.DueDate)
                throw new ArgumentException("Exam date has passed");

            // 5. SECURE: Check if already submitted or has existing session
            var existingSessions = await examStudentRepo.FindAsync(x => x.ExamId == examId && x.StudentId == studentId);
            var existing = existingSessions.FirstOrDefault();

            if (existing != null)
            {
                if (existing.IsSubmitted)
                    throw new ArgumentException("You have already submitted this exam");
                
                return existing.Id; // Return existing session ID to continue
            }

            var examStudent = new ExamStudent
            {
                ExamId = examId,
                StudentId = studentId,
                StartDate = now,
                IsSubmitted = false
            };

            await examStudentRepo.AddAsync(examStudent);
            await _unitOfWork.CompleteAsync();

            return examStudent.Id;
        }

        // ================================
        // SAVE ANSWER
        // ================================
        public async Task SaveAnswerAsync(int examStudentId, int questionId, int choiceId)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var examAnswerRepo = _unitOfWork.Repository<ExamAnswer>();

            var examStudent = await examStudentRepo.GetByIdAsync(examStudentId)
                              ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.IsSubmitted)
                throw new ArgumentException("Exam already submitted");

            var choice = await _unitOfWork.Repository<Choice>().GetByIdAsync(choiceId)
                        ?? throw new ItemNotFoundException("Invalid choice");

            var eq = await _unitOfWork.Repository<ExamQuestion>().GetByIdAsync(examQuestionId)
                ?? throw new ItemNotFoundException("Question not found in exam");

            if (choice.QuestionId != eq.QuestionId)
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
                    QuestionId = questionId,
                    ChoiceId = choiceId
                });
            }

            await _unitOfWork.CompleteAsync();
        }

        // ================================
        // SUBMIT EXAM
        // ================================
        public async Task SubmitExamAsync(int examStudentId)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var examAnswerRepo = _unitOfWork.Repository<ExamAnswer>();
            var questionRepo = _unitOfWork.Repository<Question>();
            var choiceRepo = _unitOfWork.Repository<Choice>();

            var examStudent = await examStudentRepo.GetByIdAsync(examStudentId)
                              ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.IsSubmitted)
                throw new ArgumentException("Exam already submitted");

            var answers = await examAnswerRepo
                .FindAsync(x => x.ExamStudentId == examStudentId);

            if (!answers.Any())
            {
                // If no answers, score is 0 and mark as submitted
                examStudent.Score = 0;
                examStudent.IsSubmitted = true;
                examStudent.SubmissionDate = DateTime.UtcNow;
                await examStudentRepo.UpdateAsync(examStudent);
                await _unitOfWork.CompleteAsync();
                return;
            }

            var choiceIds = answers.Select(a => a.ChoiceId).ToList();
            var choices = (await choiceRepo.FindAsync(c => choiceIds.Contains(c.Id))).ToList();

            var examQuestions = (await examQuestionRepo
                .FindAsync(x => x.ExamId == examStudent.ExamId, "Question")).ToList();

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
            examStudent.IsSubmitted = true;
            examStudent.SubmissionDate = DateTime.UtcNow;

            await examStudentRepo.UpdateAsync(examStudent);
            await _unitOfWork.CompleteAsync();
        }

        // ================================
        // GET QUESTIONS FOR SESSION (SECURE)
        // ================================
        public async Task<IEnumerable<Exam.Application.Dto.Question.QuestionForStudentDTO>> GetExamQuestionsAsync(int examStudentId)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var questionRepo = _unitOfWork.Repository<Exam.Domain.Entities.Question>();

            var examStudent = await examStudentRepo.GetByIdAsync(examStudentId)
                               ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.IsSubmitted)
                throw new ArgumentException("Exam already submitted");

            // Fetch Questions for this exam directly
            var questions = await questionRepo.FindAsync(q => q.ExamId == examStudent.ExamId, "Choices");

            return _mapper.Map<IEnumerable<Exam.Application.Dto.Question.QuestionForStudentDTO>>(questions);
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
                IsPassed = session.Exam != null && session.Score >= (session.Exam.TotalGrade / 2.0), // Example pass condition
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.IsSubmitted
            };
        }

        public async Task<Exam.Application.Dto.SubmitExam.ExamResultDTO> GetResultBySessionAsync(int examStudentId)
        {
            var examStudents = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(x => x.Id == examStudentId, "Exam", "Student");

            var session = examStudents.FirstOrDefault() ?? throw new ItemNotFoundException("Exam session not found");

            return new Exam.Application.Dto.SubmitExam.ExamResultDTO
            {
                ExamId = session.ExamId,
                StudentId = session.StudentId,
                ExamName = session.Exam?.Name ?? "Unknown",
                StudentName = session.Student != null ? $"{session.Student.FirstName} {session.Student.LastName}" : "Unknown",
                Score = session.Score,
                TotalGrade = session.Exam?.TotalGrade ?? 0,
                IsPassed = session.Exam != null && session.Score >= (session.Exam.TotalGrade / 2.0),
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.IsSubmitted
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
                IsPassed = session.Exam != null && session.Score >= (session.Exam.TotalGrade / 2.0),
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.IsSubmitted
            });
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
                IsPassed = session.Exam != null && session.Score >= (session.Exam.TotalGrade / 2.0),
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.IsSubmitted
            });
        }
    }
}