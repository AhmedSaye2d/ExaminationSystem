using AutoMapper;
using Exam.Application.Dto.Question;
using Exam.Application.Dto.SubmitExam;
using Exam.Application.Exceptions;
using Exam.Application.Services.Interfaces.IExamStudentServices;
using Exam.Domain.Entities;
using Exam.Domain.Enum;
using Exam.Domain.Interface;
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
        public async Task<StartExamResponseDTO> StartExamAsync(int examId, int studentId)
        {
            var examRepo = _unitOfWork.Repository<Exam.Domain.Entities.Exam>();
            var studentRepo = _unitOfWork.Repository<Student>();
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var courseStudentRepo = _unitOfWork.Repository<CourseStudent>();

            var exam = await examRepo.GetByIdAsync(examId)
                       ?? throw new ItemNotFoundException("Exam not found");

            if (!exam.IsPublished)
                throw new UnauthorizedAccessException("This exam is not available yet");

            var studentExists = await studentRepo.ExistsAsync(studentId);
            if (!studentExists)
                throw new ItemNotFoundException("Student not found");

            var enrollments = await courseStudentRepo.FindAsync(x => x.StudentId == studentId && x.CourseId == exam.CourseID);
            if (!enrollments.Any())
                throw new UnauthorizedAccessException("Student is not enrolled in this course");

            var now = DateTime.UtcNow;
            if (now < exam.StartDate)
                throw new ArgumentException($"Exam has not started yet. Starts at {exam.StartDate}");
            if (now > exam.DueDate)
                throw new ArgumentException("Exam date has passed");

            var existingSessions = (await examStudentRepo.FindAsync(x => x.ExamId == examId && x.StudentId == studentId)).ToList();
            var inProgressSession = existingSessions.FirstOrDefault(s => s.Status == ExamStatus.InProgress);

            ExamStudent examStudent;
            if (inProgressSession != null)
            {
                examStudent = inProgressSession;
            }
            else
            {
                var maxAttempts = exam.Settings?.MaxAttempts ?? 1;
                if (maxAttempts > 0 && existingSessions.Count >= maxAttempts)
                {
                    throw new ArgumentException($"You have reached the maximum number of attempts ({maxAttempts}) for this exam.");
                }

                examStudent = new ExamStudent
                {
                    ExamId = examId,
                    StudentId = studentId,
                    StartDate = now,
                    Status = ExamStatus.InProgress
                };

                await examStudentRepo.AddAsync(examStudent);
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Student {StudentId} started exam {ExamId}. Session ID: {SessionId}", studentId, examId, examStudent.Id);
            }

            var questions = await _unitOfWork.Repository<Question>().FindAsync(q => q.ExamId == examId, "Choices");
            var questionDtos = _mapper.Map<List<QuestionForStudentDTO>>(questions);

            var settings = exam.Settings;
            if (settings != null)
            {
                var rng = new Random(examStudent.Id);
                if (settings.ShuffleQuestions)
                {
                    questionDtos = questionDtos.OrderBy(x => rng.Next()).ToList();
                }
                if (settings.ShuffleChoices)
                {
                    foreach (var q in questionDtos)
                    {
                        q.Choices = q.Choices.OrderBy(x => rng.Next()).ToList();
                    }
                }
            }

            var totalGradeSum = questionDtos.Sum(q => q.Grade);
            var remainingSeconds = exam.Settings != null
                ? (int)Math.Max(0, (exam.Settings.DurationMinutes * 60) - (now - examStudent.StartDate).TotalSeconds)
                : 0;

            return new StartExamResponseDTO
            {
                ExamStudentId = examStudent.Id,
                ExamId = examId,
                ExamName = exam.Name ?? "No Title",
                DurationMinutes = exam.Settings?.DurationMinutes ?? 0,
                TotalGrade = totalGradeSum,
                PassingScore = exam.PassingScore,
                StartDate = examStudent.StartDate,
                ServerTime = now,
                RemainingSeconds = remainingSeconds,
                Status = "active",
                Questions = questionDtos
            };
        }

        // ================================
        // SAVE ANSWER
        // ================================
        public async Task SaveAnswerAsync(int examStudentId, int studentId, int questionId, int choiceId)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var examAnswerRepo = _unitOfWork.Repository<ExamAnswer>();

            var results = await examStudentRepo.FindWithTrackingAsync(x => x.Id == examStudentId, "Exam");
            var examStudent = results.FirstOrDefault() ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.StudentId != studentId)
                throw new UnauthorizedAccessException("This session does not belong to you");

            if (examStudent.Status == ExamStatus.Submitted)
                throw new ArgumentException("Exam already submitted");

            var choice = await _unitOfWork.Repository<Choice>().GetByIdAsync(choiceId)
                        ?? throw new ItemNotFoundException("Invalid choice");

            var question = await _unitOfWork.Repository<Question>().GetByIdAsync(questionId)
                ?? throw new ItemNotFoundException("Question not found");

            if (question.ExamId != examStudent.ExamId)
                throw new ArgumentException("Question does not belong to this exam");

            if (choice.QuestionId != questionId)
                throw new ArgumentException("Choice does not belong to this question");

            var existingAnswer = await examAnswerRepo
                .FindWithTrackingAsync(x => x.ExamStudentId == examStudentId && x.QuestionId == questionId);

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
        // SUBMIT EXAM (Updated to accept optional answers)
        // ================================
        public async Task<ExamResultDTO> SubmitExamAsync(int examStudentId, int studentId, IEnumerable<ExamAnswerDTO>? submittedAnswers = null)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var examAnswerRepo = _unitOfWork.Repository<ExamAnswer>();

            // 1. Load session with tracking (minimal includes for initial validation and updates)
            var results = await examStudentRepo.FindWithTrackingAsync(
                    x => x.Id == examStudentId,
                    "Exam",
                    "Student",
                    "ExamAnswers"
                );

            var examStudent = results.FirstOrDefault() ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.StudentId != studentId)
                throw new UnauthorizedAccessException("This session does not belong to you");

            if (examStudent.Status == ExamStatus.Submitted)
                throw new ArgumentException("Exam already submitted");

            // 2. Save/Update answers provided in the request
            if (submittedAnswers != null && submittedAnswers.Any())
            {
                foreach (var ansDto in submittedAnswers)
                {
                    if (ansDto.ChoiceId.HasValue)
                    {
                        var existing = examStudent.ExamAnswers.FirstOrDefault(a => a.QuestionId == ansDto.QuestionId);
                        if (existing != null)
                        {
                            if (existing.ChoiceId != ansDto.ChoiceId.Value)
                            {
                                existing.ChoiceId = ansDto.ChoiceId.Value;
                                existing.Choice = null; // Clear stale navigation
                                await examAnswerRepo.UpdateAsync(existing);
                            }
                        }
                        else
                        {
                            await examAnswerRepo.AddAsync(new ExamAnswer
                            {
                                ExamStudentId = examStudentId,
                                StudentId = studentId,
                                ExamId = examStudent.ExamId,
                                QuestionId = ansDto.QuestionId,
                                ChoiceId = ansDto.ChoiceId.Value
                            });
                        }
                    }
                }
                await _unitOfWork.CompleteAsync();
            }

            // 3. Robust Scoring: Fetch fresh answers from DB to ensure navigation properties (Choice, Question) are loaded correctly
            var allAnswers = await examAnswerRepo.FindAsync(
                    a => a.ExamStudentId == examStudentId,
                    "Choice",
                    "Question"
                );

            double totalScore = allAnswers
                .Where(a => a.Choice != null && a.Choice.IsCorrectAnswer)
                .Sum(a => (double)(a.Question?.Grade ?? 0));

            // 4. Update session status and score
            examStudent.Score = totalScore;
            examStudent.Status = ExamStatus.Submitted;
            examStudent.SubmissionDate = DateTime.UtcNow;

            var passingScore = examStudent.Exam?.PassingScore ?? 0;
            examStudent.IsPassed = examStudent.Score >= passingScore;

            await examStudentRepo.UpdateAsync(examStudent);

            // 5. Create a formal ExamResult record for history
            var finalResult = new ExamResult
            {
                ExamStudentId = examStudent.Id,
                StudentId = examStudent.StudentId,
                ExamId = examStudent.ExamId,
                Score = examStudent.Score,
                Total = examStudent.Exam?.TotalGrade ?? 0,
                IsPassed = examStudent.IsPassed,
                CalculatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<ExamResult>().AddAsync(finalResult);

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Exam {ExamId} submitted by student {StudentId}. Score: {Score}", examStudent.ExamId, studentId, totalScore);

            return MapToResultDto(examStudent, forceShowScore: true);
        }

        public async Task<IEnumerable<QuestionForStudentDTO>> GetExamQuestionsAsync(int examStudentId, int studentId)
        {
            var examStudentRepo = _unitOfWork.Repository<ExamStudent>();
            var results = await examStudentRepo.FindAsync(x => x.Id == examStudentId, "Exam");
            var examStudent = results.FirstOrDefault() ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.StudentId != studentId)
                throw new UnauthorizedAccessException("This session does not belong to you");

            if (examStudent.Status == ExamStatus.Submitted)
                throw new ArgumentException("Exam already submitted");

            var questions = await _unitOfWork.Repository<Question>().FindAsync(q => q.ExamId == examStudent.ExamId, "Choices");
            var questionDtos = _mapper.Map<List<QuestionForStudentDTO>>(questions);

            var settings = examStudent.Exam?.Settings;
            if (settings != null)
            {
                var rng = new Random(examStudentId);
                if (settings.ShuffleQuestions)
                    questionDtos = questionDtos.OrderBy(x => rng.Next()).ToList();

                if (settings.ShuffleChoices)
                {
                    foreach (var qDto in questionDtos)
                        qDto.Choices = qDto.Choices.OrderBy(x => rng.Next()).ToList();
                }
            }

            return questionDtos;
        }

        private ExamResultDTO MapToResultDto(ExamStudent session, bool forceShowScore = false)
        {
            var hideResult = !forceShowScore && session.Exam?.Settings?.ShowResultAfterSubmit == false;

            return new ExamResultDTO
            {
                ExamId = session.ExamId,
                StudentId = session.StudentId,
                ExamName = session.Exam?.Name ?? "Unknown",
                StudentName = session.Student != null ? $"{session.Student.FirstName} {session.Student.LastName}" : "Unknown",
                Score = hideResult ? 0 : session.Score,
                TotalGrade = session.Exam?.TotalGrade ?? 0,
                IsPassed = hideResult ? false : (session.Exam != null && session.Score >= session.Exam.PassingScore),
                StartDate = session.StartDate,
                SubmissionDate = session.SubmissionDate,
                IsSubmitted = session.Status == ExamStatus.Submitted
            };
        }

        public async Task<ExamResultDTO> GetExamResultAsync(int examId, int studentId)
        {
            var examStudents = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(x => x.ExamId == examId && x.StudentId == studentId, "Exam", "Student");

            var session = examStudents.FirstOrDefault() ?? throw new ItemNotFoundException("Exam session not found");
            return MapToResultDto(session);
        }

        public async Task<ExamResultDTO> GetResultBySessionAsync(int examStudentId, int studentId)
        {
            var examStudents = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(x => x.Id == examStudentId, "Exam", "Student");

            var session = examStudents.FirstOrDefault() ?? throw new ItemNotFoundException("Exam session not found");
            if (session.StudentId != studentId)
                throw new UnauthorizedAccessException("This result does not belong to you");

            return MapToResultDto(session);
        }

        public async Task<IEnumerable<ExamResultDTO>> GetStudentResultsAsync(int studentId)
        {
            var examStudents = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(x => x.StudentId == studentId, "Exam", "Student");

            return examStudents.Select(session => MapToResultDto(session));
        }

        public async Task<(IEnumerable<ExamResultDTO> Items, int TotalCount)> GetStudentResultsPagedAsync(int studentId, int page, int pageSize)
        {
            var (items, totalCount) = await _unitOfWork.Repository<ExamStudent>()
                .GetPagedAsync(page, pageSize, x => x.StudentId == studentId, true, "Exam", "Student");

            return (items.Select(session => MapToResultDto(session)), totalCount);
        }

        public async Task<IEnumerable<ExamResultDTO>> GetExamResultsAsync(int examId)
        {
            var examStudents = await _unitOfWork.Repository<ExamStudent>()
                .FindAsync(x => x.ExamId == examId, "Exam", "Student");

            return examStudents.Select(session => MapToResultDto(session, forceShowScore: true));
        }

        public async Task<(IEnumerable<ExamResultDTO> Items, int TotalCount)> GetExamResultsPagedAsync(int examId, int page, int pageSize)
        {
            var (items, totalCount) = await _unitOfWork.Repository<ExamStudent>()
                .GetPagedAsync(page, pageSize, x => x.ExamId == examId, true, "Exam", "Student");

            return (items.Select(session => MapToResultDto(session, forceShowScore: true)), totalCount);
        }

        public async Task<ResumeExamDTO> ResumeExamAsync(int examStudentId, int studentId)
        {
            var session = await _unitOfWork.Repository<ExamStudent>().GetByIdAsync(examStudentId) ?? throw new ItemNotFoundException("Exam session not found");

            if (session.StudentId != studentId)
                throw new UnauthorizedAccessException("This session does not belong to you");

            if (session.Status == ExamStatus.Submitted || session.Status == ExamStatus.Expired)
                throw new ArgumentException("This exam session has already ended");

            var exam = await _unitOfWork.Repository<Domain.Entities.Exam>().GetByIdAsync(session.ExamId);

            if (exam?.Settings != null)
            {
                var elapsedMinutes = (DateTime.UtcNow - session.StartDate).TotalMinutes;
                if (elapsedMinutes > exam.Settings.DurationMinutes)
                    throw new ArgumentException("The exam time has expired");
            }

            var questions = await _unitOfWork.Repository<Question>().FindAsync(q => q.ExamId == session.ExamId, "Choices");
            var answers = await _unitOfWork.Repository<ExamAnswer>().FindAsync(a => a.ExamStudentId == examStudentId);

            return new ResumeExamDTO
            {
                ExamStudentId = examStudentId,
                ExamId = session.ExamId,
                Questions = _mapper.Map<IEnumerable<QuestionForStudentDTO>>(questions),
                SavedAnswers = answers.Select(a => new ExamAnswerDTO { QuestionId = a.QuestionId, ChoiceId = a.ChoiceId }),
                RemainingMinutes = exam?.Settings != null ? Math.Max(0, exam.Settings.DurationMinutes - (int)(DateTime.UtcNow - session.StartDate).TotalMinutes) : 0
            };
        }

        public async Task<IEnumerable<StudentExamAnswerResponseDTO>> GetStudentAnswersAsync(int examStudentId, int studentId)
        {
            var results = await _unitOfWork.Repository<ExamStudent>().FindAsync(
                    x => x.Id == examStudentId,
                    "ExamAnswers.Question",
                    "ExamAnswers.Choice"
                );
            var examStudent = results.FirstOrDefault() ?? throw new ItemNotFoundException("Exam session not found");

            if (examStudent.StudentId != studentId)
                throw new UnauthorizedAccessException("This session does not belong to you");

            var answers = examStudent.ExamAnswers ?? new List<ExamAnswer>();

            return answers.Select(a => new StudentExamAnswerResponseDTO
            {
                QuestionId = a.QuestionId,
                QuestionText = a.Question?.Text ?? "N/A",
                SelectedChoiceId = a.ChoiceId,
                SelectedChoiceText = a.Choice?.Text ?? "N/A"
            });
        }

        public async Task<IEnumerable<QuestionReadDTO>> GetExamSolutionsAsync(int examId, int studentId)
        {
            var exam = await _unitOfWork.Repository<Domain.Entities.Exam>().GetByIdAsync(examId) ?? throw new ItemNotFoundException("Exam not found");

            var sessions = await _unitOfWork.Repository<ExamStudent>().FindAsync(x => x.ExamId == examId && x.StudentId == studentId);
            var submittedSession = sessions.FirstOrDefault(s => s.Status == ExamStatus.Submitted);

            if (submittedSession == null)
                throw new UnauthorizedAccessException("You must submit the exam before viewing solutions");

            if (exam.Settings != null && !exam.Settings.AllowReview)
                throw new UnauthorizedAccessException("Reviewing solutions is not allowed");

            var questions = await _unitOfWork.Repository<Question>().FindAsync(q => q.ExamId == examId, "Choices");
            return _mapper.Map<IEnumerable<QuestionReadDTO>>(questions);
        }

        public async Task<IEnumerable<StudentExamAnswerResponseDTO>> GetMyAnswersByExamIdAsync(int examId, int studentId)
        {
            var sessions = await _unitOfWork.Repository<ExamStudent>().FindAsync(
                    x => x.ExamId == examId && x.StudentId == studentId && x.Status == ExamStatus.Submitted,
                    "ExamAnswers.Question",
                    "ExamAnswers.Choice"
                );

            var session = sessions.OrderByDescending(s => s.SubmissionDate).FirstOrDefault()
                          ?? throw new ItemNotFoundException("No submitted session found");

            var answers = session.ExamAnswers ?? new List<ExamAnswer>();

            return answers.Select(a => new StudentExamAnswerResponseDTO
            {
                QuestionId = a.QuestionId,
                QuestionText = a.Question?.Text ?? "N/A",
                SelectedChoiceId = a.ChoiceId,
                SelectedChoiceText = a.Choice?.Text ?? "N/A"
            });
        }
    }
}