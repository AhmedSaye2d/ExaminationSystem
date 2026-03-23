using Exam.Domain.Entities;
using Exam.Domain.Enum;
using Exam.Domain.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Exam.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service that runs every minute and auto-submits expired exams.
    /// Ensures students cannot continue after allowed duration has elapsed.
    /// </summary>
    public class ExamAutoSubmitService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExamAutoSubmitService> _logger;
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

        public ExamAutoSubmitService(IServiceScopeFactory scopeFactory, ILogger<ExamAutoSubmitService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExamAutoSubmitService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredExamsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ExamAutoSubmitService.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task ProcessExpiredExamsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var examStudentRepo = unitOfWork.Repository<ExamStudent>();
            var examRepo = unitOfWork.Repository<Exam.Domain.Entities.Exam>();
            var questionRepo = unitOfWork.Repository<Question>();
            var examAnswerRepo = unitOfWork.Repository<ExamAnswer>();

            // Find all in-progress sessions with necessary details
            var activeSessions = await examStudentRepo.FindAsync(
                es => es.Status == ExamStatus.InProgress,
                "Exam"
            );

            var now = DateTime.UtcNow;
            int count = 0;

            foreach (var session in activeSessions)
            {
                if (session.Exam?.Settings == null) continue;

                var deadline = session.StartDate.AddMinutes(session.Exam.Settings.DurationMinutes);
                if (now < deadline) continue;

                // Time expired — auto calculate score and submit
                // BULK LOADING: Load answers with their questions AND choices in one go to avoid N+1 queries
                var answers = await examAnswerRepo.FindAsync(
                    a => a.ExamStudentId == session.Id,
                    "Question", "Choice"
                );

                double totalScore = 0;
                foreach (var answer in answers)
                {
                    if (answer.Choice?.IsCorrectAnswer == true)
                    {
                        totalScore += answer.Question?.Grade ?? 0;
                    }
                }

                session.Score = totalScore;
                session.Status = ExamStatus.Expired;
                session.SubmissionDate = now;
                
                // Consistency: Use PassingScore logic
                session.IsPassed = session.Exam.PassingScore > 0
                    ? totalScore >= session.Exam.PassingScore
                    : totalScore >= (session.Exam.TotalGrade / 2.0);

                await examStudentRepo.UpdateAsync(session);
                count++;
            }

            if (count > 0)
            {
                await unitOfWork.CompleteAsync();
                _logger.LogInformation("Auto-submitted {Count} expired exam sessions.", count);
            }
        }
    }
}
