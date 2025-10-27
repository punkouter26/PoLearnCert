namespace Po.LearnCert.Api.Features.Quiz.Infrastructure;

/// <summary>
/// Repository interface for quiz session operations.
/// </summary>
public interface IQuizSessionRepository
{
    /// <summary>
    /// Creates a new quiz session.
    /// </summary>
    Task<QuizSessionEntity> CreateSessionAsync(QuizSessionEntity session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a quiz session by ID.
    /// </summary>
    Task<QuizSessionEntity?> GetSessionByIdAsync(string userId, string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a quiz session.
    /// </summary>
    Task<QuizSessionEntity> UpdateSessionAsync(QuizSessionEntity session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sessions for a user.
    /// </summary>
    Task<IEnumerable<QuizSessionEntity>> GetUserSessionsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an answer for a session.
    /// </summary>
    Task<SessionAnswerEntity> RecordAnswerAsync(SessionAnswerEntity answer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all answers for a session.
    /// </summary>
    Task<IEnumerable<SessionAnswerEntity>> GetSessionAnswersAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific answer.
    /// </summary>
    Task<SessionAnswerEntity?> GetAnswerAsync(string sessionId, string questionId, CancellationToken cancellationToken = default);
}
