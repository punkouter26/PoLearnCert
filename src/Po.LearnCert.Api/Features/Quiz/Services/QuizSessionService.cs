using Po.LearnCert.Api.Features.Certifications.Infrastructure;
using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Api.Features.Quiz.Services.Handlers;
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
    private readonly IAnswerSubmissionHandler _answerHandler;
    private readonly IQuizCompletionHandler _completionHandler;
    private readonly ILogger<QuizSessionService> _logger;

    public QuizSessionService(
        IQuizSessionRepository sessionRepository,
        IQuestionRepository questionRepository,
        ICertificationRepository certificationRepository,
        IAnswerSubmissionHandler answerHandler,
        IQuizCompletionHandler completionHandler,
        ILogger<QuizSessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _questionRepository = questionRepository;
        _certificationRepository = certificationRepository;
        _answerHandler = answerHandler;
        _completionHandler = completionHandler;
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

        // Validate the answer submission
        var validation = await _answerHandler.ValidateAsync(userId, request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.ErrorMessage);
        }

        var session = validation.Session!;
        var question = validation.Question!;
        var selectedChoice = validation.SelectedChoice!;
        var correctChoice = validation.CorrectChoice!;

        bool isCorrect = selectedChoice.IsCorrect;

        // Record the answer and update session
        var (_, updatedSession) = await _answerHandler.RecordAnswerAsync(
            session, request, isCorrect, cancellationToken);

        // Process completion if session is done
        if (updatedSession.IsCompleted)
        {
            await _completionHandler.ProcessCompletionAsync(userId, updatedSession, cancellationToken);
        }

        // Return response
        return new SubmitAnswerResponse
        {
            IsCorrect = isCorrect,
            CorrectChoiceId = correctChoice.RowKey,
            Explanation = question.Explanation,
            CorrectAnswers = updatedSession.CorrectAnswers,
            IncorrectAnswers = updatedSession.IncorrectAnswers,
            CurrentQuestionIndex = updatedSession.CurrentQuestionIndex,
            IsSessionComplete = updatedSession.IsCompleted
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
            throw new KeyNotFoundException($"Session {sessionId} not found for user {userId}.");
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
            throw new KeyNotFoundException($"Session {sessionId} not found for user {userId}.");
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
