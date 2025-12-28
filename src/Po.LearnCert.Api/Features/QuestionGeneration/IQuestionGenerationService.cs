namespace Po.LearnCert.Api.Features.QuestionGeneration;

/// <summary>
/// Service for generating certification exam questions using AI.
/// </summary>
public interface IQuestionGenerationService
{
    /// <summary>
    /// Generates questions for a specific subtopic.
    /// </summary>
    /// <param name="certificationId">The certification ID.</param>
    /// <param name="subtopicId">The subtopic ID.</param>
    /// <param name="subtopicName">The subtopic name.</param>
    /// <param name="count">Number of questions to generate.</param>
    /// <param name="progress">Progress reporter for real-time updates.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generation result with success/failure counts.</returns>
    Task<QuestionGenerationResult> GenerateQuestionsAsync(
        string certificationId,
        string subtopicId,
        string subtopicName,
        int count,
        IProgress<QuestionGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates questions for all subtopics of a certification.
    /// </summary>
    Task<QuestionGenerationResult> GenerateQuestionsForCertificationAsync(
        string certificationId,
        int questionsPerSubtopic,
        IProgress<QuestionGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of question generation operation.
/// </summary>
public record QuestionGenerationResult(
    int TotalGenerated,
    int TotalFailed,
    TimeSpan Duration,
    List<string> Errors);

/// <summary>
/// Progress update during question generation.
/// </summary>
public record QuestionGenerationProgress(
    string CurrentSubtopic,
    int CurrentQuestion,
    int TotalQuestions,
    int Generated,
    int Failed,
    string? LastError = null);
