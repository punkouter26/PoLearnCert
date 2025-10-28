using Po.LearnCert.Shared.Contracts;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Api.Features.Quiz.Services;

/// <summary>
/// Service for managing quiz sessions and answer submissions.
/// </summary>
public interface IQuizSessionService
{
    /// <summary>
    /// Creates a new quiz session for a user.
    /// </summary>
    /// <param name="userId">The user ID creating the session.</param>
    /// <param name="request">The session creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created quiz session.</returns>
    Task<QuizSessionDto> CreateSessionAsync(string userId, CreateQuizSessionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits an answer for a quiz question.
    /// </summary>
    /// <param name="userId">The user ID submitting the answer.</param>
    /// <param name="request">The answer submission request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The answer submission response with feedback.</returns>
    Task<SubmitAnswerResponse> SubmitAnswerAsync(string userId, SubmitAnswerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the final results for a completed quiz session.
    /// </summary>
    /// <param name="userId">The user ID who owns the session.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The quiz results.</returns>
    Task<QuizResultDto> GetSessionResultsAsync(string userId, string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the details of an active or completed quiz session.
    /// </summary>
    /// <param name="userId">The user ID who owns the session.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The quiz session details.</returns>
    Task<QuizSessionDto> GetSessionDetailsAsync(string userId, string sessionId, CancellationToken cancellationToken = default);
}
