using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IExamStudentServices;
using Exam.Domain;
using Exam.Domain.Entities;
using Exam.Domain.Interface;

namespace Exam.Application.Services.Implementation
{
    public class StudentExamService : IStudentExamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentExamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================================
        // START EXAM
        // ================================
        public async Task<int> StartExamAsync(int examId, int studentId)
        {
            var exam = await _unitOfWork.Repository<Exam.Domain.Entities.Exam>().GetByIdAsync(examId)
                       ?? throw new ItemNotFoundException("Exam not found");

            var studentExists = await _unitOfWork.Repository<Student>().ExistsAsync(studentId);
            if (!studentExists)
                throw new ItemNotFoundException("Student not found");

            if (DateTime.UtcNow < exam.StartDate)
                throw new ArgumentException("Exam has not started yet");

            if (DateTime.UtcNow > exam.DueDate)
                throw new ArgumentException("Exam has ended");

            var existingSession = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(x => x.ExamId == examId && x.StudentId == studentId);

            if (existingSession.Any())
                throw new ArgumentException("You already started this exam");

            var examStudent = new ExamStudent
            {
                ExamId = examId,
                StudentId = studentId,
                IsSubmitted = false,
                Score = 0,
                StartDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ExamStudent>().AddAsync(examStudent);
            await _unitOfWork.CompleteAsync();

            return examStudent.Id;
        }

        // ================================
        // SAVE ANSWER
        // ================================
        public async Task SaveAnswerAsync(int examStudentId, int examQuestionId, int choiceId)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var examAnswerRepo = _unitOfWork.Repository<ExamAnswer>();

            var examStudent = await examStudentRepo.GetByIdAsync(examStudentId)
                              ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.IsSubmitted)
                throw new ArgumentException("Exam already submitted");

            var choice = await _unitOfWork.Repository<Choice>().GetByIdAsync(choiceId)
                        ?? throw new ItemNotFoundException("Invalid choice");

            var existingAnswer = await examAnswerRepo
                .FindAsync(x => x.ExamStudentId == examStudentId
                             && x.ExamQuestionId == examQuestionId);

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
                    ExamQuestionId = examQuestionId,
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
            var examQuestionRepo = _unitOfWork.Repository<ExamQuestion>();
            var choiceRepo = _unitOfWork.Repository<Choice>();

            var examStudent = await examStudentRepo.GetByIdAsync(examStudentId)
                              ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.IsSubmitted)
                throw new ArgumentException("Exam already submitted");

            var answers = await examAnswerRepo
                .FindAsync(x => x.ExamStudentId == examStudentId);

            if (!answers.Any())
                throw new ArgumentException("No answers found");

            var allChoices = await choiceRepo.GetAllAsync();
            var examQuestions = await examQuestionRepo
                .FindAsync(x => x.ExamId == examStudent.ExamId);

            double totalScore = 0;

            foreach (var answer in answers)
            {
                var choice = allChoices.FirstOrDefault(c => c.Id == answer.ChoiceId);

                if (choice?.IsCorrectAnswer == true)
                {
                    var examQuestion = examQuestions
                        .FirstOrDefault(q => q.Id == answer.ExamQuestionId);

                    if (examQuestion?.Question != null)
                        totalScore += examQuestion.Question.Grade;
                }
            }

            examStudent.Score = totalScore;
            examStudent.IsSubmitted = true;
            examStudent.SubmissionDate = DateTime.UtcNow;

            await examStudentRepo.UpdateAsync(examStudent);
            await _unitOfWork.CompleteAsync();
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