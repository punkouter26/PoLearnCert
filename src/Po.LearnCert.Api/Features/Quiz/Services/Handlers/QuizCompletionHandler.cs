using Microsoft.AspNetCore.Identity;
using Po.LearnCert.Api.Features.Authentication.Infrastructure;
using Po.LearnCert.Api.Features.Leaderboards.Services;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Api.Features.Statistics;

namespace Po.LearnCert.Api.Features.Quiz.Services.Handlers;

/// <summary>
/// Handles post-quiz completion tasks like statistics and leaderboard updates.
/// </summary>
public interface IQuizCompletionHandler
{
    /// <summary>
    /// Processes completion of a quiz session.
    /// </summary>
    Task ProcessCompletionAsync(
        string userId,
        QuizSessionEntity session,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the score percentage for a session.
    /// </summary>
    int CalculateScorePercentage(QuizSessionEntity session);
}

/// <summary>
/// Handles post-quiz completion tasks like statistics and leaderboard updates.
/// </summary>
public class QuizCompletionHandler : IQuizCompletionHandler
{
    private readonly IUserStatisticsService _statisticsService;
    private readonly LeaderboardService _leaderboardService;
    private readonly UserManager<UserEntity> _userManager;
    private readonly ILogger<QuizCompletionHandler> _logger;

    public QuizCompletionHandler(
        IUserStatisticsService statisticsService,
        LeaderboardService leaderboardService,
        UserManager<UserEntity> userManager,
        ILogger<QuizCompletionHandler> logger)
    {
        _statisticsService = statisticsService;
        _leaderboardService = leaderboardService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task ProcessCompletionAsync(
        string userId,
        QuizSessionEntity session,
        CancellationToken cancellationToken = default)
    {
        if (!session.IsCompleted || !session.CompletedAt.HasValue)
        {
            return;
        }

        var scorePercentage = CalculateScorePercentage(session);

        // Update statistics (non-blocking)
        await UpdateStatisticsAsync(userId, session.SessionId);

        // Update leaderboards (non-blocking)
        await UpdateLeaderboardAsync(userId, session, scorePercentage);
    }

    /// <inheritdoc/>
    public int CalculateScorePercentage(QuizSessionEntity session)
    {
        int totalQuestions = session.CorrectAnswers + session.IncorrectAnswers;
        return totalQuestions > 0
            ? (int)Math.Round((double)session.CorrectAnswers / totalQuestions * 100)
            : 0;
    }

    private async Task UpdateStatisticsAsync(string userId, string sessionId)
    {
        try
        {
            await _statisticsService.UpdateStatisticsAfterSessionAsync(userId, sessionId);
            _logger.LogInformation("Statistics updated for completed session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update statistics for session {SessionId}", sessionId);
            // Don't fail the request if statistics update fails
        }
    }

    private async Task UpdateLeaderboardAsync(string userId, QuizSessionEntity session, int scorePercentage)
    {
        try
        {
            // Get the actual username from the user manager
            var user = await _userManager.FindByIdAsync(userId);
            var username = user?.UserName ?? userId;

            await _leaderboardService.UpdateLeaderboardAsync(
                userId,
                username,
                session.CertificationId,
                scorePercentage,
                session.CompletedAt!.Value.DateTime);

            _logger.LogInformation(
                "Leaderboard updated for user {UserId} on certification {CertId} with score {Score}",
                userId, session.CertificationId, scorePercentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update leaderboard for session {SessionId}", session.SessionId);
            // Don't fail the request if leaderboard update fails
        }
    }
}
