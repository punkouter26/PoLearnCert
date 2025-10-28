using Po.LearnCert.Shared.Contracts;
using Po.LearnCert.Shared.Models;

namespace Po.LearnCert.Client.Features.Quiz.Services;

/// <summary>
/// Service interface for quiz session operations.
/// </summary>
public interface IQuizSessionService
{
    /// <summary>
    /// Creates a new quiz session.
    /// </summary>
    Task<QuizSessionDto> CreateSessionAsync(CreateQuizSessionRequest request);

    /// <summary>
    /// Submits an answer for a quiz question.
    /// </summary>
    Task<SubmitAnswerResponse> SubmitAnswerAsync(SubmitAnswerRequest request);

    /// <summary>
    /// Gets the details of a quiz session.
    /// </summary>
    Task<QuizSessionDto> GetSessionAsync(string sessionId);

    /// <summary>
    /// Gets the final results for a completed quiz session.
    /// </summary>
    Task<QuizResultDto> GetSessionResultsAsync(string sessionId);

    /// <summary>
    /// Gets a question by ID.
    /// </summary>
    Task<QuestionDto> GetQuestionAsync(string certificationId, string questionId);
}
