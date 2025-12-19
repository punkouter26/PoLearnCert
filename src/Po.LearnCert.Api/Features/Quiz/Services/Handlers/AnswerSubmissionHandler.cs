using Po.LearnCert.Api.Features.Quiz.Infrastructure;
using Po.LearnCert.Shared.Contracts;

namespace Po.LearnCert.Api.Features.Quiz.Services.Handlers;

/// <summary>
/// Result of validating an answer submission.
/// </summary>
public record AnswerValidationResult(
    bool IsValid,
    string? ErrorMessage = null,
    QuizSessionEntity? Session = null,
    QuestionEntity? Question = null,
    AnswerChoiceEntity? SelectedChoice = null,
    AnswerChoiceEntity? CorrectChoice = null);

/// <summary>
/// Handles answer submission validation and recording.
/// </summary>
public interface IAnswerSubmissionHandler
{
    /// <summary>
    /// Validates an answer submission request.
    /// </summary>
    Task<AnswerValidationResult> ValidateAsync(
        string userId,
        SubmitAnswerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an answer and updates session progress.
    /// </summary>
    Task<(bool IsCorrect, QuizSessionEntity UpdatedSession)> RecordAnswerAsync(
        QuizSessionEntity session,
        SubmitAnswerRequest request,
        bool isCorrect,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Handles answer submission validation and recording.
/// </summary>
public class AnswerSubmissionHandler : IAnswerSubmissionHandler
{
    private readonly IQuizSessionRepository _sessionRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ILogger<AnswerSubmissionHandler> _logger;

    public AnswerSubmissionHandler(
        IQuizSessionRepository sessionRepository,
        IQuestionRepository questionRepository,
        ILogger<AnswerSubmissionHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _questionRepository = questionRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AnswerValidationResult> ValidateAsync(
        string userId,
        SubmitAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate session exists and is active
        var session = await _sessionRepository.GetSessionByIdAsync(userId, request.SessionId, cancellationToken);
        if (session == null)
        {
            return new AnswerValidationResult(false, $"Session {request.SessionId} not found for user {userId}.");
        }

        if (session.IsCompleted)
        {
            return new AnswerValidationResult(false, $"Session {request.SessionId} is already completed.");
        }

        // Validate question exists
        var question = await _questionRepository.GetQuestionByIdAsync(
            session.CertificationId,
            request.QuestionId,
            cancellationToken);

        if (question == null)
        {
            return new AnswerValidationResult(false, $"Question {request.QuestionId} not found.");
        }

        // Validate selected choice exists
        var choices = await _questionRepository.GetAnswerChoicesAsync(request.QuestionId, cancellationToken);
        var selectedChoice = choices.FirstOrDefault(c => c.RowKey == request.SelectedChoiceId);
        var correctChoice = choices.FirstOrDefault(c => c.IsCorrect);

        if (selectedChoice == null)
        {
            return new AnswerValidationResult(false, $"Choice {request.SelectedChoiceId} not found.");
        }

        if (correctChoice == null)
        {
            return new AnswerValidationResult(false, $"No correct answer found for question {request.QuestionId}.");
        }

        return new AnswerValidationResult(true, null, session, question, selectedChoice, correctChoice);
    }

    /// <inheritdoc/>
    public async Task<(bool IsCorrect, QuizSessionEntity UpdatedSession)> RecordAnswerAsync(
        QuizSessionEntity session,
        SubmitAnswerRequest request,
        bool isCorrect,
        CancellationToken cancellationToken = default)
    {
        // Create answer entity
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
        }

        await _sessionRepository.UpdateSessionAsync(session, cancellationToken);

        _logger.LogInformation(
            "Answer recorded for session {SessionId}, question {QuestionId}, correct: {IsCorrect}",
            request.SessionId, request.QuestionId, isCorrect);

        return (isCorrect, session);
    }
}
