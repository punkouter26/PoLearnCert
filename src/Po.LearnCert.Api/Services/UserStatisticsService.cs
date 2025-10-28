using Po.LearnCert.Api.Entities;
using Po.LearnCert.Api.Repositories;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Services;

/// <summary>
/// Service implementation for user statistics operations.
/// </summary>
public class UserStatisticsService : IUserStatisticsService
{
    private readonly IUserStatisticsRepository _statisticsRepository;
    private readonly IQuizSessionRepository _sessionRepository;
    private readonly ICertificationRepository _certificationRepository;
    private readonly ISubtopicRepository _subtopicRepository;

    public UserStatisticsService(
        IUserStatisticsRepository statisticsRepository,
        IQuizSessionRepository sessionRepository,
        ICertificationRepository certificationRepository,
        ISubtopicRepository subtopicRepository)
    {
        _statisticsRepository = statisticsRepository;
        _sessionRepository = sessionRepository;
        _certificationRepository = certificationRepository;
        _subtopicRepository = subtopicRepository;
    }

    public async Task<UserStatisticsDto> GetUserStatisticsAsync(string userId)
    {
        // Get overall statistics
        var userStats = await _statisticsRepository.GetUserStatisticsAsync(userId);

        // Get certification performance
        var certPerformance = await _statisticsRepository.GetCertificationPerformanceAsync(userId);

        // If no statistics exist, create default empty statistics
        if (userStats == null)
        {
            userStats = UserStatisticsEntity.Create(userId);
        }

        var result = new UserStatisticsDto
        {
            UserId = userId,
            TotalSessions = userStats.TotalSessions,
            TotalQuestionsAnswered = userStats.TotalQuestionsAnswered,
            TotalCorrectAnswers = userStats.TotalCorrectAnswers,
            OverallAccuracy = (decimal)userStats.OverallAccuracy,
            AverageScore = (decimal)userStats.AverageScore,
            BestScore = (decimal)userStats.BestScore,
            LastSessionDate = !string.IsNullOrEmpty(userStats.LastSessionDate)
                ? DateTime.Parse(userStats.LastSessionDate)
                : null,
            TotalStudyTime = TimeSpan.FromMinutes(userStats.TotalStudyTimeMinutes),
            CertificationPerformance = certPerformance.Select(cp => new CertificationPerformanceDto
            {
                CertificationId = cp.CertificationId,
                CertificationName = cp.CertificationName,
                SessionCount = cp.SessionCount,
                AverageScore = (decimal)cp.AverageScore,
                BestScore = (decimal)cp.BestScore,
                QuestionsAnswered = cp.QuestionsAnswered,
                Accuracy = (decimal)cp.Accuracy
            }).ToList()
        };

        return result;
    }

    public async Task<IEnumerable<SubtopicPerformanceDto>> GetSubtopicPerformanceAsync(string userId, string? certificationId = null)
    {
        IEnumerable<SubtopicPerformanceEntity> subtopicPerformance;

        if (!string.IsNullOrEmpty(certificationId))
        {
            subtopicPerformance = await _statisticsRepository.GetSubtopicPerformanceAsync(userId, certificationId);
        }
        else
        {
            subtopicPerformance = await _statisticsRepository.GetSubtopicPerformanceAsync(userId);
        }

        return subtopicPerformance.Select(sp => new SubtopicPerformanceDto
        {
            SubtopicId = sp.SubtopicId,
            SubtopicName = sp.SubtopicName,
            CertificationId = sp.CertificationId,
            CertificationName = sp.CertificationName,
            QuestionsAnswered = sp.QuestionsAnswered,
            CorrectAnswers = sp.CorrectAnswers,
            Accuracy = (decimal)sp.Accuracy,
            PerformanceLevel = (PerformanceLevel)sp.PerformanceLevel,
            PracticeCount = sp.PracticeCount,
            LastPracticedDate = !string.IsNullOrEmpty(sp.LastPracticedDate)
                ? DateTime.Parse(sp.LastPracticedDate)
                : null,
            AverageTimePerQuestion = TimeSpan.FromSeconds(sp.AverageTimePerQuestionSeconds)
        });
    }

    public async Task<SessionHistoryDto> GetSessionHistoryAsync(string userId, DateRange? dateRange = null, int? limit = null)
    {
        // Get all sessions for the user
        var sessions = await _sessionRepository.GetUserSessionsAsync(userId);

        // Apply date range filter if specified
        if (dateRange != null)
        {
            sessions = sessions.Where(s => s.StartedAt >= dateRange.StartDate && s.StartedAt <= dateRange.EndDate);
        }

        // Order by most recent first
        sessions = sessions.OrderByDescending(s => s.StartedAt);

        // Apply limit if specified
        if (limit.HasValue)
        {
            sessions = sessions.Take(limit.Value);
        }

        var sessionList = sessions.ToList();
        var sessionSummaries = new List<SessionSummaryDto>();

        foreach (var session in sessionList)
        {
            // Get certification name
            var certification = await _certificationRepository.GetCertificationByIdAsync(session.CertificationId);

            // Calculate total questions from question IDs
            var questionIds = session.QuestionIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var totalQuestions = questionIds.Length;
            var questionsAnswered = session.CorrectAnswers + session.IncorrectAnswers;

            sessionSummaries.Add(new SessionSummaryDto
            {
                SessionId = session.SessionId,
                CertificationId = session.CertificationId,
                CertificationName = certification?.Name ?? "Unknown",
                StartedAt = session.StartedAt.DateTime,
                CompletedAt = session.CompletedAt?.DateTime,
                TotalQuestions = totalQuestions,
                QuestionsAnswered = questionsAnswered,
                CorrectAnswers = session.CorrectAnswers,
                ScorePercentage = totalQuestions > 0
                    ? (decimal)session.CorrectAnswers / totalQuestions * 100
                    : 0,
                Duration = session.CompletedAt?.Subtract(session.StartedAt) ?? TimeSpan.Zero,
                Status = session.CompletedAt.HasValue ? SessionStatus.Completed : SessionStatus.InProgress
            });
        }

        return new SessionHistoryDto
        {
            UserId = userId,
            Sessions = sessionSummaries,
            TotalSessions = sessionSummaries.Count,
            DateRange = dateRange
        };
    }

    public async Task UpdateStatisticsAfterSessionAsync(string userId, string sessionId)
    {
        // Get the completed session
        var session = await _sessionRepository.GetSessionByIdAsync(userId, sessionId);
        if (session == null || !session.CompletedAt.HasValue)
        {
            return;
        }

        // Get certification info
        var certification = await _certificationRepository.GetCertificationByIdAsync(session.CertificationId);
        if (certification == null)
        {
            return;
        }

        // Calculate session statistics from session entity
        var questionIds = session.QuestionIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var totalQuestions = questionIds.Length;
        var correctAnswers = session.CorrectAnswers;
        var scorePercentage = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0;
        var sessionDuration = session.CompletedAt.Value.Subtract(session.StartedAt);

        // Update overall user statistics
        await UpdateOverallStatisticsAsync(userId, totalQuestions, correctAnswers, scorePercentage, sessionDuration);

        // Update certification performance
        await UpdateCertificationPerformanceAsync(userId, session.CertificationId, certification.Name,
            totalQuestions, correctAnswers, scorePercentage);

        // For now, skip subtopic performance update since we need session answers data
        // This would require extending the session storage to track subtopic-level answers
    }

    public PerformanceLevel CalculatePerformanceLevel(decimal accuracy)
    {
        return accuracy switch
        {
            >= 95m => PerformanceLevel.Mastered,
            >= 85m => PerformanceLevel.Excellent,
            >= 70m => PerformanceLevel.Good,
            >= 50m => PerformanceLevel.Basic,
            _ => PerformanceLevel.NeedsImprovement
        };
    }

    private async Task UpdateOverallStatisticsAsync(string userId, int questionsAnswered, int correctAnswers,
        double scorePercentage, TimeSpan sessionDuration)
    {
        var userStats = await _statisticsRepository.GetUserStatisticsAsync(userId)
            ?? UserStatisticsEntity.Create(userId);

        // Update counters
        userStats.TotalSessions++;
        userStats.TotalQuestionsAnswered += questionsAnswered;
        userStats.TotalCorrectAnswers += correctAnswers;
        userStats.TotalStudyTimeMinutes += (long)sessionDuration.TotalMinutes;

        // Update accuracy
        userStats.OverallAccuracy = userStats.TotalQuestionsAnswered > 0
            ? (double)userStats.TotalCorrectAnswers / userStats.TotalQuestionsAnswered * 100
            : 0;

        // Update scores
        userStats.AverageScore = (userStats.AverageScore * (userStats.TotalSessions - 1) + scorePercentage) / userStats.TotalSessions;
        userStats.BestScore = Math.Max(userStats.BestScore, scorePercentage);

        // Update last session date
        userStats.LastSessionDate = DateTime.UtcNow.ToString("O");

        await _statisticsRepository.UpdateUserStatisticsAsync(userStats);
    }

    private async Task UpdateCertificationPerformanceAsync(string userId, string certificationId,
        string certificationName, int questionsAnswered, int correctAnswers, double scorePercentage)
    {
        var certPerformance = await _statisticsRepository.GetCertificationPerformanceAsync(userId, certificationId)
            ?? CertificationPerformanceEntity.Create(userId, certificationId, certificationName);

        // Update counters
        certPerformance.SessionCount++;
        certPerformance.QuestionsAnswered += questionsAnswered;
        certPerformance.CorrectAnswers += correctAnswers;

        // Update accuracy
        certPerformance.Accuracy = certPerformance.QuestionsAnswered > 0
            ? (double)certPerformance.CorrectAnswers / certPerformance.QuestionsAnswered * 100
            : 0;

        // Update scores
        certPerformance.AverageScore = (certPerformance.AverageScore * (certPerformance.SessionCount - 1) + scorePercentage) / certPerformance.SessionCount;
        certPerformance.BestScore = Math.Max(certPerformance.BestScore, scorePercentage);

        await _statisticsRepository.UpdateCertificationPerformanceAsync(certPerformance);
    }
}