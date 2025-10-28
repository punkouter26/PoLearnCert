using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Api.Features.Leaderboards.Services;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Api.Services;
using Po.LearnCert.Shared.Contracts;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Features.Quiz.Services;

/// <summary>
/// Service for managing quiz sessions and answer submissions.
/// </summary>
public class QuizSessionService : IQuizSessionService
{
    private readonly IQuizSessionRepository _sessionRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ICertificationRepository _certificationRepository;
    private readonly IUserStatisticsService _statisticsService;
    private readonly LeaderboardService _leaderboardService;
    private readonly ILogger<QuizSessionService> _logger;

    public QuizSessionService(
        IQuizSessionRepository sessionRepository,
        IQuestionRepository questionRepository,
        ICertificationRepository certificationRepository,
        IUserStatisticsService statisticsService,
        LeaderboardService leaderboardService,
        ILogger<QuizSessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _questionRepository = questionRepository;
        _certificationRepository = certificationRepository;
        _statisticsService = statisticsService;
        _leaderboardService = leaderboardService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<QuizSessionDto> CreateSessionAsync(
        string userId,
        CreateQuizSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating quiz session for user {UserId}, certification {CertificationId}, subtopic {SubtopicId}",
            userId, request.CertificationId, request.SubtopicId);

        // Validate certification exists
        var certification = await _certificationRepository.GetCertificationByIdAsync(request.CertificationId, cancellationToken);
        if (certification == null)
        {
            throw new InvalidOperationException($"Certification {request.CertificationId} not found.");
        }

        // Validate subtopic if specified
        string? subtopicId = null;
        if (!string.IsNullOrEmpty(request.SubtopicId))
        {
            var subtopic = await _certificationRepository.GetSubtopicByIdAsync(
                request.CertificationId,
                request.SubtopicId,
                cancellationToken);

            if (subtopic == null)
            {
                throw new InvalidOperationException(
                    $"Subtopic {request.SubtopicId} not found in certification {request.CertificationId}.");
            }

            subtopicId = request.SubtopicId;
        }

        // Get random questions
        var questions = await _questionRepository.GetRandomQuestionsAsync(
            request.CertificationId,
            subtopicId,
            request.QuestionCount,
            cancellationToken);

        var questionsList = questions.ToList();
        if (questionsList.Count == 0)
        {
            throw new InvalidOperationException(
                $"No questions available for certification {request.CertificationId}" +
                (subtopicId != null ? $" and subtopic {subtopicId}" : ""));
        }

        // Create session entity
        var sessionEntity = new QuizSessionEntity
        {
            PartitionKey = userId,
            RowKey = Guid.NewGuid().ToString(),
            UserId = userId,
            CertificationId = request.CertificationId,
            SubtopicId = subtopicId,
            QuestionIds = string.Join(",", questionsList.Select(q => q.RowKey)),
            CurrentQuestionIndex = 0,
            CorrectAnswers = 0,
            IncorrectAnswers = 0,
            StartedAt = DateTimeOffset.UtcNow,
            IsCompleted = false,
            Timestamp = DateTimeOffset.UtcNow
        };

        var createdEntity = await _sessionRepository.CreateSessionAsync(sessionEntity, cancellationToken);

        _logger.LogInformation(
            "Created quiz session {SessionId} for user {UserId} with {QuestionCount} questions",
            createdEntity.RowKey, userId, questionsList.Count);

        // Map to DTO
        return new QuizSessionDto
        {
            Id = createdEntity.RowKey,
            UserId = createdEntity.UserId,
            CertificationId = createdEntity.CertificationId,
            SubtopicId = createdEntity.SubtopicId,
            QuestionIds = createdEntity.QuestionIds.Split(',').ToList(),
            CurrentQuestionIndex = createdEntity.CurrentQuestionIndex,
            CorrectAnswers = createdEntity.CorrectAnswers,
            IncorrectAnswers = createdEntity.IncorrectAnswers,
            StartedAt = createdEntity.StartedAt,
            CompletedAt = createdEntity.CompletedAt,
            IsCompleted = createdEntity.IsCompleted
        };
    }

    /// <inheritdoc/>
    public async Task<SubmitAnswerResponse> SubmitAnswerAsync(
        string userId,
        SubmitAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "User {UserId} submitting answer for session {SessionId}, question {QuestionId}",
            userId, request.SessionId, request.QuestionId);

        // Get session
        var session = await _sessionRepository.GetSessionByIdAsync(userId, request.SessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException($"Session {request.SessionId} not found for user {userId}.");
        }

        if (session.IsCompleted)
        {
            throw new InvalidOperationException($"Session {request.SessionId} is already completed.");
        }

        // Get question and choices
        var question = await _questionRepository.GetQuestionByIdAsync(
            session.CertificationId, // Use certification ID as partition key
            request.QuestionId,
            cancellationToken);

        if (question == null)
        {
            throw new InvalidOperationException($"Question {request.QuestionId} not found.");
        }

        var choices = await _questionRepository.GetAnswerChoicesAsync(request.QuestionId, cancellationToken);
        var selectedChoice = choices.FirstOrDefault(c => c.RowKey == request.SelectedChoiceId);
        var correctChoice = choices.FirstOrDefault(c => c.IsCorrect);

        if (selectedChoice == null)
        {
            throw new InvalidOperationException($"Choice {request.SelectedChoiceId} not found.");
        }

        if (correctChoice == null)
        {
            throw new InvalidOperationException($"No correct answer found for question {request.QuestionId}.");
        }

        bool isCorrect = selectedChoice.IsCorrect;

        // Record answer
        var answerEntity = new SessionAnswerEntity
        {
            PartitionKey = request.SessionId,
            RowKey = request.QuestionId,
            SessionId = request.SessionId,
            QuestionId = request.QuestionId,
            SelectedChoiceId = request.SelectedChoiceId,
            IsCorrect = isCorrect,
            AnsweredAt = DateTimeOffset.UtcNow,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _sessionRepository.RecordAnswerAsync(answerEntity, cancellationToken);

        // Update session progress
        if (isCorrect)
        {
            session.CorrectAnswers++;
        }
        else
        {
            session.IncorrectAnswers++;
        }

        session.CurrentQuestionIndex++;

        // Check if session is complete
        var questionIds = session.QuestionIds.Split(',');
        if (session.CurrentQuestionIndex >= questionIds.Length)
        {
            session.IsCompleted = true;
            session.CompletedAt = DateTimeOffset.UtcNow;

            // Calculate final score
            int totalQuestions = session.CorrectAnswers + session.IncorrectAnswers;
            int scorePercentage = totalQuestions > 0
                ? (int)Math.Round((double)session.CorrectAnswers / totalQuestions * 100)
                : 0;

            // Update user statistics
            try
            {
                await _statisticsService.UpdateStatisticsAfterSessionAsync(userId, request.SessionId);
                _logger.LogInformation("Statistics updated for completed session {SessionId}", request.SessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update statistics for session {SessionId}", request.SessionId);
                // Don't fail the request if statistics update fails
            }

            // Update leaderboards
            try
            {
                // Get user information - in production this would come from the auth context
                var username = userId; // TODO: Get actual username from user service/auth context
                
                await _leaderboardService.UpdateLeaderboardAsync(
                    userId,
                    username,
                    session.CertificationId,
                    scorePercentage,
                    session.CompletedAt.Value.DateTime);

                _logger.LogInformation(
                    "Leaderboard updated for user {UserId} on certification {CertId} with score {Score}",
                    userId, session.CertificationId, scorePercentage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update leaderboard for session {SessionId}", request.SessionId);
                // Don't fail the request if leaderboard update fails
            }
        }

        await _sessionRepository.UpdateSessionAsync(session, cancellationToken);

        _logger.LogInformation(
            "Answer recorded for session {SessionId}, question {QuestionId}, correct: {IsCorrect}",
            request.SessionId, request.QuestionId, isCorrect);

        // Return response
        return new SubmitAnswerResponse
        {
            IsCorrect = isCorrect,
            CorrectChoiceId = correctChoice.RowKey,
            Explanation = question.Explanation,
            CorrectAnswers = session.CorrectAnswers,
            IncorrectAnswers = session.IncorrectAnswers,
            CurrentQuestionIndex = session.CurrentQuestionIndex,
            IsSessionComplete = session.IsCompleted
        };
    }

    /// <inheritdoc/>
    public async Task<QuizResultDto> GetSessionResultsAsync(
        string userId,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting results for session {SessionId}, user {UserId}",
            sessionId, userId);

        var session = await _sessionRepository.GetSessionByIdAsync(userId, sessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found for user {userId}.");
        }

        if (!session.IsCompleted)
        {
            throw new InvalidOperationException($"Session {sessionId} is not completed yet.");
        }

        // Get certification details
        var certification = await _certificationRepository.GetCertificationByIdAsync(session.CertificationId, cancellationToken);
        string certificationName = certification?.Name ?? session.CertificationId;

        string? subtopicName = null;
        if (!string.IsNullOrEmpty(session.SubtopicId))
        {
            var subtopic = await _certificationRepository.GetSubtopicByIdAsync(
                session.CertificationId,
                session.SubtopicId,
                cancellationToken);
            subtopicName = subtopic?.Name;
        }

        int totalQuestions = session.CorrectAnswers + session.IncorrectAnswers;
        double scorePercentage = totalQuestions > 0
            ? Math.Round((double)session.CorrectAnswers / totalQuestions * 100, 2)
            : 0;

        int durationSeconds = session.CompletedAt.HasValue
            ? (int)(session.CompletedAt.Value - session.StartedAt).TotalSeconds
            : 0;

        return new QuizResultDto
        {
            SessionId = session.RowKey,
            CertificationName = certificationName,
            SubtopicName = subtopicName,
            CorrectAnswers = session.CorrectAnswers,
            IncorrectAnswers = session.IncorrectAnswers,
            TotalQuestions = totalQuestions,
            ScorePercentage = (decimal)scorePercentage,
            DurationSeconds = durationSeconds,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt!.Value
        };
    }

    /// <inheritdoc/>
    public async Task<QuizSessionDto> GetSessionDetailsAsync(
        string userId,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting details for session {SessionId}, user {UserId}",
            sessionId, userId);

        var session = await _sessionRepository.GetSessionByIdAsync(userId, sessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found for user {userId}.");
        }

        return new QuizSessionDto
        {
            Id = session.RowKey,
            UserId = session.UserId,
            CertificationId = session.CertificationId,
            SubtopicId = session.SubtopicId,
            QuestionIds = session.QuestionIds.Split(',').ToList(),
            CurrentQuestionIndex = session.CurrentQuestionIndex,
            CorrectAnswers = session.CorrectAnswers,
            IncorrectAnswers = session.IncorrectAnswers,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            IsCompleted = session.IsCompleted
        };
    }
}
