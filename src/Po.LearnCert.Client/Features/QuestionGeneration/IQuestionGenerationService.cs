namespace Po.LearnCert.Client.Features.QuestionGeneration;

/// <summary>
/// Client service for question generation API.
/// </summary>
public interface IQuestionGenerationService
{
    /// <summary>
    /// Generates questions for a specific subtopic.
    /// </summary>
    Task<GenerationResultDto> GenerateForSubtopicAsync(
        string certificationId,
        string subtopicId,
        string subtopicName,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates questions for all subtopics of a certification.
    /// </summary>
    Task<GenerationResultDto> GenerateForCertificationAsync(
        string certificationId,
        int questionsPerSubtopic,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of question generation.
/// </summary>
public record GenerationResultDto(
    int TotalGenerated,
    int TotalFailed,
    double DurationSeconds,
    List<string> Errors);

/// <summary>
/// Request to generate questions for a subtopic.
/// </summary>
public record GenerateForSubtopicRequest(
    string CertificationId,
    string SubtopicId,
    string SubtopicName,
    int Count);

/// <summary>
/// Request to generate questions for a certification.
/// </summary>
public record GenerateForCertificationRequest(
    string CertificationId,
    int QuestionsPerSubtopic);
