using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IExamStudentServices;
using Exam.Domain;
using Exam.Domain.Entities;

namespace Exam.Application.Services.Implementation
{
    public class StudentExamService : IStudentExamService
    {
        private readonly IGenericRepository<Exam.Domain.Entities.Exam> _examRepo;
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IGenericRepository<ExamStudent> _examStudentRepo;
        private readonly IGenericRepository<ExamQuestion> _examQuestionRepo;
        private readonly IGenericRepository<ExamAnswer> _examAnswerRepo;
        private readonly IGenericRepository<Choice> _choiceRepo;

        public StudentExamService(
            IGenericRepository<Exam.Domain.Entities.Exam> examRepo,
            IGenericRepository<Student> studentRepo,
            IGenericRepository<ExamStudent> examStudentRepo,
            IGenericRepository<ExamQuestion> examQuestionRepo,
            IGenericRepository<ExamAnswer> examAnswerRepo,
            IGenericRepository<Choice> choiceRepo)
        {
            _examRepo = examRepo;
            _studentRepo = studentRepo;
            _examStudentRepo = examStudentRepo;
            _examQuestionRepo = examQuestionRepo;
            _examAnswerRepo = examAnswerRepo;
            _choiceRepo = choiceRepo;
        }

        // ================================
        // START EXAM
        // ================================
        public async Task<int> StartExamAsync(int examId, int studentId)
        {
            var exam = await _examRepo.GetByIdAsync(examId)
                       ?? throw new ItemNotFoundException("Exam not found");

            var studentExists = await _studentRepo.ExistsAsync(studentId);
            if (!studentExists)
                throw new ItemNotFoundException("Student not found");

            if (DateTime.UtcNow < exam.StartDate)
                throw new ArgumentException("Exam has not started yet");

            if (DateTime.UtcNow > exam.DueDate)
                throw new ArgumentException("Exam has ended");

            var existingSession = await _examStudentRepo
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

            await _examStudentRepo.AddAsync(examStudent);

            return examStudent.Id;
        }

        // ================================
        // SAVE ANSWER
        // ================================
        public async Task SaveAnswerAsync(int examStudentId, int examQuestionId, int choiceId)
        {
            var examStudent = await _examStudentRepo.GetByIdAsync(examStudentId)
                              ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.IsSubmitted)
                throw new ArgumentException("Exam already submitted");

            var choice = await _choiceRepo.GetByIdAsync(choiceId)
                        ?? throw new ItemNotFoundException("Invalid choice");

            var existingAnswer = await _examAnswerRepo
                .FindAsync(x => x.ExamStudentId == examStudentId
                             && x.ExamQuestionId == examQuestionId);

            if (existingAnswer.Any())
            {
                var answer = existingAnswer.First();
                answer.ChoiceId = choiceId;
                await _examAnswerRepo.UpdateAsync(answer);
            }
            else
            {
                await _examAnswerRepo.AddAsync(new ExamAnswer
                {
                    ExamStudentId = examStudentId,
                    ExamQuestionId = examQuestionId,
                    ChoiceId = choiceId
                });
            }
        }

        // ================================
        // SUBMIT EXAM
        // ================================
        public async Task SubmitExamAsync(int examStudentId)
        {
            var examStudent = await _examStudentRepo.GetByIdAsync(examStudentId)
                              ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.IsSubmitted)
                throw new ArgumentException("Exam already submitted");

            var answers = await _examAnswerRepo
                .FindAsync(x => x.ExamStudentId == examStudentId);

            if (!answers.Any())
                throw new ArgumentException("No answers found");

            var allChoices = await _choiceRepo.GetAllAsync();
            var examQuestions = await _examQuestionRepo
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

            await _examStudentRepo.UpdateAsync(examStudent);
        }
    }
}